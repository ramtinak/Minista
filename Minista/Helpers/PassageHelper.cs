using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Minista.Helpers
{
    class PassageHelperX : IDisposable
    {
        public Tuple<List<Inline>, bool> GetInlines(string text, TypedEventHandler<Hyperlink, HyperlinkClickEventArgs> hyperLinkAction, Color? defaultColor = null, Color? hyperColor = null)
        {
            var rtl = string.IsNullOrEmpty(text) ? false : text.HasPersianChar();
            var inlines = new List<Inline>();

            if (hyperColor == null)
                hyperColor = Helper.GetColorFromHex("#ff5897fc");
            if (defaultColor == null)
                defaultColor = ((SolidColorBrush)Application.Current.Resources["DefaultForegroundColor"]).Color;
            try
            {
                var splitter = new List<string>(text.Split(new[] { "\r\n", Environment.NewLine, "\n", "\r" }, StringSplitOptions.None));
                //var r = GetRun(null, defaultColor.Value);

                var replacedText = string.Join(" ⤴⥈⥽⥶⍬ ", splitter);
                var parsedText = GetParsedPassage(replacedText);

                parsedText.ForEach(item =>
                {
                    if (item.PassageType == PassageType.Text)
                    {
                        if (item.Text.Contains("@") || item.Text.Contains("#") || item.Text.Contains("http://") ||
                            item.Text.Contains("https://") || item.Text.Contains("www.") || item.Text.Contains("https://"))
                        {
                            var input = item.Text;
                            var matches = Regex.Matches(input, @"(@[a-zA-Z0-9_\-\.]+)|(#[\d\w_]*)",
                                RegexOptions.IgnoreCase);
                            if (matches?.Count > 0)
                            {
                                for (int i = 0; i < matches.Count; i++)
                                {
                                    var inp = matches[i].ToString();
                                    input = input.Replace(inp, $" {inp} ");
                                }
                                //input = input.Trim();
                                input = input.Replace("  ", " ");

                                var list2 = new List<string>(input.Split(new string[] { " " }, StringSplitOptions.None));
                                list2.ForEach(item2 =>
                                {
                                    if (item2.Contains("@") || item2.StartsWith("#") || item2.StartsWith("{") || item2.Contains("http://") ||
                                    item2.Contains("https://") || item2.Contains("www."))
                                    {
                                        var hyper = GetHyperlink(item2, hyperLinkAction, hyperColor.Value);
                                        inlines.Add(hyper);
                                    }
                                    else if (item2.Contains("https:\\"))
                                    {
                                        var hyper = GetHyperlink(item2.Replace("https:\\","").Replace("⥽⍬⥶⍬⥽", " "), hyperLinkAction, hyperColor.Value);
                                        inlines.Add(hyper);
                                    }
                                    else
                                    {
                                        inlines.Add(GetRun(item2, defaultColor.Value));
                                    }
                                });

                            }
                            else
                            {
                                var hyper = GetHyperlink(item.Text, hyperLinkAction, hyperColor.Value);

                                inlines.Add(hyper);
                            }
                            //p.Inlines.Add(new Run() { Text = " " });
                        }
                        else
                        {
                            inlines.Add(GetRun(item.Text, defaultColor.Value));
                            //p.Inlines.Add(new Run() { Text = " " });
                        }
                    }
                    else if (item.PassageType == PassageType.Space)
                        inlines.Add(GetRun(" ", defaultColor.Value));
                    else
                    {
                        if (/*item.Text.StartsWith("@@@@@")*/ item.PassageType == PassageType.NewLine)
                            inlines.Add(new LineBreak());
                        else
                        {
                            var input = item.Text;
                            var matches = Regex.Matches(input, @"(@[a-zA-Z0-9_\-\.]+)|(#[\d\w_]*)",
                                RegexOptions.IgnoreCase);
                            if (matches?.Count > 0)
                            {
                                for (int i = 0; i < matches.Count; i++)
                                {
                                    var inp = matches[i].ToString();
                                    input = input.Replace(inp, $" {inp} ");
                                }
                                //input = input.Trim();
                                input = input.Replace("  ", " ");

                                var list2 = new List<string>(input.Split(new string[] { " " }, StringSplitOptions.None));
                                list2.ForEach(item2 =>
                                {
                                    if (item2.Contains("@") || item2.StartsWith("#") || item2.StartsWith("{") || item2.Contains("http://") ||
                                    item2.Contains("https://") || item2.Contains("www."))
                                    {
                                        var hyper = GetHyperlink(item2, hyperLinkAction, hyperColor.Value);
                                        inlines.Add(hyper);
                                    }
                                    else if (item2.Contains("https:\\"))
                                    {
                                        var hyper = GetHyperlink(item2.Replace("https:\\", "").Replace("⥽⍬⥶⍬⥽", " "), hyperLinkAction, hyperColor.Value);
                                        inlines.Add(hyper);
                                    }
                                    else
                                    {
                                        inlines.Add(GetRun(item2, defaultColor.Value));
                                    }
                                });

                            }
                            else
                            {
                                //input = input.Replace("   ", " ");

                                //var hyper = new Hyperlink
                                //{
                                //    Foreground = new SolidColorBrush(hyperColor.Value)
                                //};
                                //hyper.Inlines.Add(new Run()
                                //{
                                //    Text = /*Environment.NewLine +*/ item.Text,
                                //    Foreground = new SolidColorBrush(hyperColor.Value),

                                //});

                                //hyper.UnderlineStyle = UnderlineStyle.None;
                                //hyper.Click += hyperLinkAction;
                                var hyper = GetHyperlink(input, hyperLinkAction, hyperColor.Value);
                                inlines.Add(hyper);
                                inlines.Add(GetRun(" ", defaultColor.Value));
                            }
                        }

                    }
                });
            }
            catch
            {
                inlines.Clear();
                inlines.Add(GetRun(text, defaultColor.Value));
            }

            return new Tuple<List<Inline>, bool>(inlines, rtl);
        }
        Run GetRun(string text, Color defaultColor)
        {
            return new Run()
            {
                Text = text,
                //Foreground = new SolidColorBrush(defaultColor)
            };
        }
        Hyperlink GetHyperlink(string text, TypedEventHandler<Hyperlink, HyperlinkClickEventArgs> hyperLinkAction, Color hyperColor)
        {

            if (text.Contains("https:\\"))
                text = text.Replace("https:\\", "").Replace("⥽⍬⥶⍬⥽", " ");

            var hyper = new Hyperlink
            {
                //Foreground = new SolidColorBrush(hyperColor)
            };

            hyper.Inlines.Add(new Run()
            {
                Text = /*Environment.NewLine +*/ text,
                //Foreground = new SolidColorBrush(hyperColor),

            });

            hyper.UnderlineStyle = UnderlineStyle.None;
            hyper.Click += hyperLinkAction;
            return hyper;
        }

        public List<Passage> GetParsedPassage(string text)
        {
            var parsedList = new List<Passage>();
            try
            {
                var list = new List<string>(text.Split(new string[] { " "/*, Environment.NewLine*/ }, StringSplitOptions.None));
                list.ForEach(item =>
                {
                    if (item.StartsWith("http://") || item.StartsWith("https://") || item.StartsWith("www."))
                        parsedList.Add(new Passage { Text = item, PassageType = PassageType.Url });
                    else if (item.StartsWith("https:\\"))
                        parsedList.Add(new Passage { Text = item, PassageType = PassageType.Activity });
                    else if (item.StartsWith("#"))
                        parsedList.Add(new Passage { Text = item, PassageType = PassageType.Hashtag });
                    else if (item.StartsWith("⤴⥈⥽⥶⍬"))
                        parsedList.Add(new Passage { PassageType = PassageType.NewLine });
                    else if (item.StartsWith("@"))
                        parsedList.Add(new Passage { Text = item, PassageType = PassageType.User });
                    else if (item.StartsWith("{") && item.EndsWith("}")) //{mitsubishi.savaran|000000|1|user?id=7039536546}
                    {
                        parsedList.Add(new Passage { Text = item.Substring(1, item.IndexOf("|")-1), PassageType = PassageType.User });
                    }
                    else
                    {
                        parsedList.Add(new Passage { Text = item, PassageType = PassageType.Text });
                    }
                    parsedList.Add(new Passage { PassageType = PassageType.Space });

                });
            }
            catch { }

            return parsedList;
        }

        bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            disposed = true;
        }
    }

    public class Passage
    {
        public string Text { get; set; } = string.Empty;
        public PassageType PassageType { get; set; }
    }
    public enum PassageType
    {
        Text = -1,
        Hashtag,
        Url,
        User,
        NewLine,
        Space,
        Activity
    }
}