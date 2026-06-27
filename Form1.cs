using JsonOptimizerApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IeltsJsonOptimizer
{
    public partial class Form1 : Form
    {
        // شمارنده برای تولید شناسه‌های ثابت و قابل پیش‌بینی
        private int _nodeCounter = 1;

        // نگهداری JSON اصلی در حافظه فرم تا زمان ادغام
        private JsonNode _originalJsonNode;

        public Form1()
        {
            InitializeComponent();
        }

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
                    string tempId = $"item_{_nodeCounter}";
                    _nodeCounter++;

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

        private string GetTextFromSpans(JsonArray spans)
        {
            List<string> spanTexts = new List<string>();
            foreach (var sNode in spans.OfType<JsonObject>())
            {
                if (sNode.ContainsKey("Type") && sNode["Type"]?.ToString() == "text" && sNode.ContainsKey("Content") && !sNode.ContainsKey("_isConsumed"))
                {
                    spanTexts.Add(sNode["Content"].ToString());
                    sNode["_isConsumed"] = true;
                }
            }
            return string.Join("", spanTexts);
        }

        private void MergeTranslationsRecursive(JsonNode node, AiResponseWrapper aiData)
        {
            if (node is JsonObject obj)
            {
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
                        // اصلاح: افزودن ترجمه‌ها به عنوان گره متنی جدید (رشته)
                        if (translatedItem["translationFa"] != null)
                            obj["translationFa"] = translatedItem["translationFa"].ToString();

                        if (translatedItem["translationAr"] != null)
                            obj["translationAr"] = translatedItem["translationAr"].ToString();
                    }

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

        private void btnGeneratePayload_Click(object sender, EventArgs e)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string inputJson = rtbInput.Text;

                // ریست کردن شمارنده در هر بار تولید Payload
                _nodeCounter = 1;

                // پارس کردن فایل اصلی
                _originalJsonNode = JsonNode.Parse(inputJson);

                // استفاده از همان فایل اصلی برای استخراج (جلوگیری از باگ رفرنس)
                JsonArray originalArray = _originalJsonNode.AsArray();

                JsonArray payloadForGemini = new JsonArray();
                ExtractPayloadsRecursive(originalArray, payloadForGemini);

                if (payloadForGemini.Count == 0)
                {
                    MessageBox.Show("هیچ Paragraph دارای Span متنی در ورودی یافت نشد!");
                    return;
                }

                rtbJsonToSend.Text = payloadForGemini.ToJsonString(options);
                MessageBox.Show("Payload با موفقیت تولید شد.");
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
                string aiResponseText = rtbAiResponse.Text;

                if (string.IsNullOrWhiteSpace(aiResponseText))
                {
                    MessageBox.Show("لطفاً پاسخ هوش مصنوعی را در کادر مربوطه Paste کنید.");
                    return;
                }

                if (_originalJsonNode == null)
                {
                    MessageBox.Show("فایل اصلی لود نشده است. لطفاً ابتدا مرحله تولید Payload را انجام دهید.");
                    return;
                }

                string parsableJson = "{\"Data\":" + aiResponseText + "}";
                var aiData = JsonSerializer.Deserialize<AiResponseWrapper>(parsableJson);

                if (aiData?.Data != null)
                {
                    // ادغام داده‌ها
                    MergeTranslationsRecursive(_originalJsonNode, aiData);

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    // تولید خروجی آرایه اصلی
                    string finalArrayJson = _originalJsonNode.ToJsonString(options);

                    // فرمت‌دهی سفارشی خروجی نهایی (چسباندن Interactives بدون آکولاد والد)
                    string customFormatOutput = finalArrayJson;
                    if (aiData.Interactives != null)
                    {
                        string interactivesJson = JsonSerializer.Serialize(aiData.Interactives, options);
                        int lastBracketIndex = finalArrayJson.LastIndexOf(']');

                        if (lastBracketIndex >= 0)
                        {
                            customFormatOutput = finalArrayJson.Substring(0, lastBracketIndex)
                                               + "],\n\"Interactives\": \n"
                                               + interactivesJson;
                        }
                    }

                    rtbOutput.Text = customFormatOutput;
                    MessageBox.Show("ادغام با موفقیت انجام شد!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در ادغام اطلاعات (شاید فرمت JSON دریافتی معتبر نیست): " + ex.Message);
            }
        }
    }
}