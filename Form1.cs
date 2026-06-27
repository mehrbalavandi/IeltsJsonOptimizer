using JsonOptimizerApp;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IeltsJsonOptimizer
{
    public partial class Form1 : Form
    {// شمارنده برای تولید شناسه‌های ثابت و قابل پیش‌بینی
        private int _nodeCounter = 1;

        // نگهداری JSON اصلی در حافظه فرم تا زمان ادغام
        private JsonNode _originalJsonNode;
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
        /// متد بازگشتی برای جستجوی عمیق JSON، استخراج و چسباندن متن‌ها برای تولید allContents
        /// </summary>
        private void ExtractPayloadsRecursive(JsonNode node, JsonArray payloadForGemini)
        {
            if (node is JsonObject obj)
            {
                bool hasValidParagraphs = false;
                bool hasValidDirectSpans = false;

                if (obj.ContainsKey("Paragraphs") && obj["Paragraphs"] is JsonArray paragraphsArray)
                {
                    hasValidParagraphs = paragraphsArray.OfType<JsonObject>().Any(p =>
                        p.ContainsKey("Spans") && p["Spans"] is JsonArray spansArray &&
                        spansArray.OfType<JsonObject>().Any(s => s.ContainsKey("Type") && s["Type"]?.ToString() == "text" && !s.ContainsKey("_isConsumed"))
                    );
                }
                else if (obj.ContainsKey("Spans") && obj["Spans"] is JsonArray directSpansArray)
                {
                    hasValidDirectSpans = directSpansArray.OfType<JsonObject>().Any(s =>
                        s.ContainsKey("Type") && s["Type"]?.ToString() == "text" && !s.ContainsKey("_isConsumed")
                    );
                }

                if (hasValidParagraphs || hasValidDirectSpans)
                {
                    // تغییر مهم: استفاده از شمارنده به جای GUID
                    string tempId = $"item_{_nodeCounter}";
                    _nodeCounter++; // افزایش شمارنده برای آیتم بعدی

                    obj["_tempId"] = tempId;

                    string allContents = "";

                    if (hasValidParagraphs)
                    {
                        allContents = GetTextFromParagraphs(obj["Paragraphs"] as JsonArray);
                    }
                    else if (hasValidDirectSpans)
                    {
                        allContents = GetTextFromSpans(obj["Spans"] as JsonArray);
                    }

                    if (!string.IsNullOrWhiteSpace(allContents))
                    {
                        var lightItem = new JsonObject
                        {
                            ["id"] = tempId,
                            ["allContents"] = allContents.Trim()
                        };
                        payloadForGemini.Add(lightItem);
                    }
                    else
                    {
                        obj.Remove("_tempId");
                    }
                }

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
        /// استخراج تمام متن‌ها از آرایه Paragraphs و چسباندن آن‌ها با خط جدید
        /// </summary>
        private string GetTextFromParagraphs(JsonArray paragraphs)
        {
            List<string> paragraphTexts = new List<string>();
            foreach (var pNode in paragraphs.OfType<JsonObject>())
            {
                if (pNode.ContainsKey("Spans") && pNode["Spans"] is JsonArray spansArray)
                {
                    string spanText = GetTextFromSpans(spansArray);
                    if (!string.IsNullOrWhiteSpace(spanText))
                    {
                        paragraphTexts.Add(spanText);
                    }
                }
            }
            return string.Join("\n", paragraphTexts);
        }

        /// <summary>
        /// استخراج تمام متن‌ها از آرایه Spans و چسباندن مستقیم آن‌ها
        /// </summary>
        private string GetTextFromSpans(JsonArray spans)
        {
            List<string> spanTexts = new List<string>();
            foreach (var sNode in spans.OfType<JsonObject>())
            {
                // فقط در صورتی متن را می‌گیریم که مصرف نشده باشد
                if (sNode.ContainsKey("Type") && sNode["Type"]?.ToString() == "text" && sNode.ContainsKey("Content") && !sNode.ContainsKey("_isConsumed"))
                {
                    spanTexts.Add(sNode["Content"].ToString());

                    // بلافاصله این گره را علامت‌گذاری می‌کنیم تا در پیمایش‌های عمیق‌تر دوباره خوانده نشود
                    sNode["_isConsumed"] = true;
                }
            }
            return string.Join("", spanTexts);
        }
        private void MergeTranslationsRecursive(JsonNode node, AiResponseWrapper aiData)
        {
            if (node is JsonObject obj)
            {
                // پاکسازی فلگ مصرف‌شدگی
                if (obj.ContainsKey("_isConsumed"))
                {
                    obj.Remove("_isConsumed");
                }

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

        private void btnGeneratePayload_Click(object sender, EventArgs e)
        {
            try
            {

                var options = new JsonSerializerOptions { WriteIndented = true };
                string inputJson = rtbInput.Text;
                _originalJsonNode = JsonNode.Parse(inputJson);

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


                rtbJsonToSend.Text = payloadForGemini.ToJsonString(options);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در تولید ساختار: " + ex.Message);
            }
        }

        private void btnMergeAiResponse_Click(object sender, EventArgs e)
        {
            try
            {
                // خواندن متنی که به صورت دستی در RichTextBox دوم Paste شده است
                string aiResponseText = rtbAiResponse.Text;

                if (string.IsNullOrWhiteSpace(aiResponseText))
                {
                    MessageBox.Show("لطفاً پاسخ هوش مصنوعی را در کادر مربوطه Paste کنید.");
                    return;
                }

                if (_originalJsonNode == null)
                {
                    MessageBox.Show("فایل اصلی لود نشده است. لطفاً ابتدا مرحله استخراج را انجام دهید.");
                    return;
                }
                string parsableJson = "{\"Data\":" + aiResponseText + "}";
                var aiData = JsonSerializer.Deserialize<AiResponseWrapper>(parsableJson);

                // تبدیل متن دریافتی به ساختار مد نظر ما (AiResponseWrapper کلاسی است که قبلاً داشتید)
                //var aiData = JsonSerializer.Deserialize<AiResponseWrapper>(aiResponseText);

                if (aiData?.Data != null)
                {
                    // فراخوانی متد ادغام (همان متدی که تگ‌های isConsumed و tempId را پاک می‌کرد)
                    MergeTranslationsRecursive(_originalJsonNode, aiData);

                    // خروجی نهایی و ترجمه شده آماده است! آن را ذخیره یا نمایش دهید
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string finalOutput = _originalJsonNode.ToJsonString(options);

                    // مثلاً ذخیره در یک فایل جدید یا نمایش در یک RichTextBox سوم
                    //System.IO.File.WriteAllText("output_translated.json", finalOutput);
                    rtbOutput.Text = finalOutput;

                    MessageBox.Show("ادغام با موفقیت انجام شد و فایل نهایی ساخته شد!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ادغام اطلاعات (شاید فرمت JSON دریافتی از هوش مصنوعی معتبر نیست): " + ex.Message);
            }
        }
    }
}