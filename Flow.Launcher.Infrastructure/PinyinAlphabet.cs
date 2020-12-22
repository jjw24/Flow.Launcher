﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Flow.Launcher.Infrastructure.UserSettings;
using ToolGood.Words.Pinyin;

namespace Flow.Launcher.Infrastructure
{
    public interface IAlphabet
    {
        string Translate(string stringToTranslate, out bool translated);
    }

    public class PinyinAlphabet : IAlphabet
    {
        private ConcurrentDictionary<string, string> _pinyinCache = new ConcurrentDictionary<string, string>();
        private Settings _settings;

        public void Initialize([NotNull] Settings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string Translate(string content, out bool translated)
        {
            if (_settings.ShouldUsePinyin)
            {
                if (!_pinyinCache.ContainsKey(content))
                {
                    if (WordsHelper.HasChinese(content))
                    {
                        translated = true;

                        var resultList = WordsHelper.GetPinyinList(content);

                        StringBuilder resultBuilder = new StringBuilder();

                        bool pre = false;

                        for (int i = 0; i < resultList.Length; i++)
                        {
                            if (content[i] >= 0x3400 && content[i] <= 0x9FD5)
                            {
                                resultBuilder.Append(' ');
                                resultBuilder.Append(resultList[i]);
                                pre = true;
                            }
                            else
                            {
                                if (pre)
                                {
                                    pre = false;
                                    resultBuilder.Append(' ');
                                }
                                resultBuilder.Append(resultList[i]);
                            }
                        }

                        return _pinyinCache[content] = resultBuilder.ToString();
                    }
                    else
                    {
                        translated = false;
                        return content;
                    }
                }
                else
                {
                    translated = true;
                    return _pinyinCache[content];
                }
            }
            else
            {
                translated = false;
                return content;
            }
        }
    }
}