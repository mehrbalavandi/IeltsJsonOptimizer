using JsonOptimizerApp;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IeltsJsonOptimizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private async void btnProcess_Click(object sender, EventArgs e)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string inputJson = rtbInput.Text;

                JsonNode rootNode = JsonNode.Parse(inputJson);
                JsonArray originalArray = rootNode.AsArray();

                // ۱. استخراج بازگشتی: تمام گره‌های تو در تو بررسی می‌شوند
                JsonArray payloadForGemini = new JsonArray();
                ExtractPayloadsRecursive(originalArray, payloadForGemini);

                string jsonToSend = payloadForGemini.ToJsonString(options);

                // اگر پس از جستجو چیزی پیدا نشد، متوقف می‌شویم
                if (payloadForGemini.Count == 0)
                {
                    MessageBox.Show("هیچ Paragraph دارای Span متنی در ورودی یافت نشد!");
                    return;
                }

                rtbOutput.Text = "در حال پردازش پرامپت آیلتس...\n" + jsonToSend; // نمایش موقت برای تست

                // ۲. دریافت پاسخ
                string rawAiResponse = await SimulateGeminiApiAsync(jsonToSend);

                // ۳. پارس کردن امن پاسخ هوش مصنوعی
                string parsableJson = "{\"Data\":" + rawAiResponse + "}";
                var aiData = JsonSerializer.Deserialize<AiResponseWrapper>(parsableJson);

                // ۴. ادغام بازگشتی ترجمه‌ها در سرتاسر درخت JSON
                MergeTranslationsRecursive(originalArray, aiData);

                // ۵. تولید خروجی نهایی سفارشی
                string finalArrayJson = originalArray.ToJsonString(options);
                string interactivesJson = JsonSerializer.Serialize(aiData.Interactives, options);

                int lastBracketIndex = finalArrayJson.LastIndexOf(']');
                string customFormatOutput = finalArrayJson.Substring(0, lastBracketIndex)
                                          + "],\n\"Interactives\": \n"
                                          + interactivesJson;

                rtbOutput.Text = customFormatOutput;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا: {ex.Message}");
            }
        }

        /// <summary>
        /// متد بازگشتی برای جستجوی عمیق JSON و استخراج فیلدهای متنی به صورت کاملاً پاکسازی شده
        /// </summary>
        private void ExtractPayloadsRecursive(JsonNode node, JsonArray payloadForGemini)
        {
            if (node is JsonObject obj)
            {
                bool hasValidParagraphs = false;
                bool hasValidDirectSpans = false;

                // بررسی حالت اول (وجود Paragraphs)
                if (obj.ContainsKey("Paragraphs") && obj["Paragraphs"] is JsonArray paragraphsArray)
                {
                    hasValidParagraphs = paragraphsArray.OfType<JsonObject>().Any(p =>
                        p.ContainsKey("Spans") && p["Spans"] is JsonArray spansArray &&
                        spansArray.OfType<JsonObject>().Any(s => s.ContainsKey("Type") && s["Type"]?.ToString() == "text")
                    );
                }
                // بررسی حالت دوم (وجود Spans مستقیم)
                else if (obj.ContainsKey("Spans") && obj["Spans"] is JsonArray directSpansArray)
                {
                    hasValidDirectSpans = directSpansArray.OfType<JsonObject>().Any(s =>
                        s.ContainsKey("Type") && s["Type"]?.ToString() == "text"
                    );
                }

                if (hasValidParagraphs || hasValidDirectSpans)
                {
                    string tempId = Guid.NewGuid().ToString();
                    obj["_tempId"] = tempId;

                    var lightItem = new JsonObject { ["id"] = tempId };

                    // به جای کپی کردن کل ساختار، فقط نسخه پاکسازی شده (فیلتر شده) را اضافه می‌کنیم
                    if (hasValidParagraphs)
                    {
                        lightItem["Paragraphs"] = BuildCleanParagraphs(obj["Paragraphs"] as JsonArray);
                    }
                    else if (hasValidDirectSpans)
                    {
                        lightItem["Spans"] = BuildCleanSpans(obj["Spans"] as JsonArray);
                    }

                    payloadForGemini.Add(lightItem);
                }

                // جستجو در عمق تمام فیلدهای این آبجکت برای پیدا کردن متن‌های مخفی شده 
                // (مثلاً اگر داخل یک Paragraph جدولی باشد که خودش متن دارد)
                foreach (var kvp in obj.ToList())
                {
                    ExtractPayloadsRecursive(kvp.Value, payloadForGemini);
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    ExtractPayloadsRecursive(item, payloadForGemini);
                }
            }
        }

        /// <summary>
        /// فیلتر کردن Paragraphs: حذف همه چیز به جز Spans متنی
        /// </summary>
        private JsonArray BuildCleanParagraphs(JsonArray originalParagraphs)
        {
            var cleanParagraphs = new JsonArray();
            foreach (var pNode in originalParagraphs.OfType<JsonObject>())
            {
                if (pNode.ContainsKey("Spans") && pNode["Spans"] is JsonArray spansArray)
                {
                    var cleanSpans = BuildCleanSpans(spansArray);
                    // فقط در صورتی پاراگراف را اضافه می‌کنیم که محتوای متنی در دل آن باقی مانده باشد
                    if (cleanSpans.Count > 0)
                    {
                        cleanParagraphs.Add(new JsonObject { ["Spans"] = cleanSpans });
                    }
                }
            }
            return cleanParagraphs;
        }

        /// <summary>
        /// فیلتر کردن Spans: استخراج Type و Content و حذف رنگ، فونت، استایل و...
        /// </summary>
        private JsonArray BuildCleanSpans(JsonArray originalSpans)
        {
            var cleanSpans = new JsonArray();
            foreach (var sNode in originalSpans.OfType<JsonObject>())
            {
                if (sNode.ContainsKey("Type") && sNode["Type"]?.ToString() == "text" && sNode.ContainsKey("Content"))
                {
                    cleanSpans.Add(new JsonObject
                    {
                        ["Type"] = "text",
                        ["Content"] = sNode["Content"].DeepClone()
                    });
                }
            }
            return cleanSpans;
        }
        /// <summary>
        /// متد بازگشتی برای جستجوی عمیق و ادغام ترجمه‌ها
        /// </summary>
        private void MergeTranslationsRecursive(JsonNode node, AiResponseWrapper aiData)
        {
            if (node is JsonObject obj)
            {
                // اگر این همون آبجکتیه که موقع استخراج بهش شناسه موقت دادیم
                if (obj.TryGetPropertyValue("_tempId", out JsonNode idNode))
                {
                    string id = idNode.ToString();
                    var translatedItem = aiData.Data.FirstOrDefault(x => x["id"]?.ToString() == id);
                    if (translatedItem != null)
                    {
                        if (translatedItem["translationFa"] != null)
                            obj["translationFa"] = translatedItem["translationFa"].DeepClone();

                        if (translatedItem["translationAr"] != null)
                            obj["translationAr"] = translatedItem["translationAr"].DeepClone();
                    }
                    // پاکسازی شناسه موقت
                    obj.Remove("_tempId");
                }

                foreach (var kvp in obj.ToList())
                {
                    MergeTranslationsRecursive(kvp.Value, aiData);
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    MergeTranslationsRecursive(item, aiData);
                }
            }
        }

        // متد شبیه‌ساز (همان متد اصلاح‌شده در پیام قبلی)
        private async Task<string> SimulateGeminiApiAsync(string payloadText)
        {
            await Task.Delay(500);

            string generatedId = "شناسه_تست";
            try
            {
                var payloadArray = JsonNode.Parse(payloadText) as JsonArray;
                if (payloadArray != null && payloadArray.Count > 0)
                {
                    generatedId = payloadArray[0]["id"]?.ToString() ?? "شناسه_تست";
                }
            }
            catch { }

            string aiSimulatedString = $@"
            [
              {{
                ""id"": ""{generatedId}"",
                ""Paragraphs"": [],
                ""translationFa"": ""این یک متن آکادمیک برای آیلتس است."",
                ""translationAr"": ""هذا نص أكاديمي لاختبار الآيلتس.""
              }}
            ],
            ""Interactives"": [
              {{
                ""exactText"": ""Academic Context"",
                ""translationFa"": ""بافت دانشگاهی"",
                ""translationAr"": ""السياق الأكاديمي"",
                ""pronounceFa"": ""آکادمیک کانتکست"",
                ""pronounceAr"": ""أكاديميك كونتيكست"",
                ""explanationFa"": ""کالوکیشن پرکاربرد در رایتینگ تسک ۲"",
                ""explanationAr"": ""متلازمة شائعة في الكتابة المهمة 2"",
                ""cefrLevel"": ""C1""
              }}
            ]";

            return aiSimulatedString;
        }
    }
}