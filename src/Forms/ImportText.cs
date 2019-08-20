﻿using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class ImportText : Form
    {
        private Subtitle _subtitle;
        private Subtitle _subtitleInput;
        private string _videoFileName;
        private readonly Timer _refreshTimer = new Timer();
        private readonly bool _exit;
        private int _startFromNumber = 1;

        public Subtitle FixedSubtitle => _subtitle;
        public SubtitleFormat Format { get; set; }
        public string VideoFileName => _videoFileName;

        public ImportText(string fileName = null, Form parentForm = null)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.ImportText.Title;
            groupBoxImportText.Text = Configuration.Settings.Language.ImportText.Title;
            buttonOpenText.Text = Configuration.Settings.Language.ImportText.OpenTextFile;
            groupBoxImportOptions.Text = Configuration.Settings.Language.ImportText.ImportOptions;
            groupBoxSplitting.Text = Configuration.Settings.Language.ImportText.Splitting;
            radioButtonAutoSplit.Text = Configuration.Settings.Language.ImportText.AutoSplitText;
            radioButtonLineMode.Text = string.Empty;
            comboBoxLineMode.Items.Clear();
            comboBoxLineMode.Items.Add(Configuration.Settings.Language.ImportText.OneLineIsOneSubtitle);
            comboBoxLineMode.Items.Add(Configuration.Settings.Language.ImportText.TwoLinesAreOneSubtitle);
            comboBoxLineMode.SelectedIndex = 0;
            labelLineBreak.Text = Configuration.Settings.Language.ImportText.LineBreak;
            columnHeaderFName.Text = Configuration.Settings.Language.JoinSubtitles.FileName;
            columnHeaderSize.Text = Configuration.Settings.Language.General.Size;
            comboBoxLineBreak.Left = labelLineBreak.Left + labelLineBreak.Width + 3;
            comboBoxLineBreak.Width = groupBoxSplitting.Width - comboBoxLineBreak.Left - 5;
            checkBoxMultipleFiles.AutoSize = true;
            checkBoxMultipleFiles.Left = buttonOpenText.Left - checkBoxMultipleFiles.Width - 9;
            checkBoxMultipleFiles.Text = Configuration.Settings.Language.ImportText.OneSubtitleIsOneFile;
            listViewInputFiles.Visible = false;
            labelSubMaxLen.Text = Configuration.Settings.Language.Settings.SubtitleLineMaximumLength;
            numericUpDownSubtitleLineMaximumLength.Left = labelSubMaxLen.Left + labelSubMaxLen.Width + 3;

            radioButtonSplitAtBlankLines.Text = Configuration.Settings.Language.ImportText.SplitAtBlankLines;
            checkBoxMergeShortLines.Text = Configuration.Settings.Language.ImportText.MergeShortLines;
            checkBoxRemoveEmptyLines.Text = Configuration.Settings.Language.ImportText.RemoveEmptyLines;
            checkBoxRemoveLinesWithoutLetters.Text = Configuration.Settings.Language.ImportText.RemoveLinesWithoutLetters;
            checkBoxAutoSplitRemoveLinesNoLetters.Text = Configuration.Settings.Language.ImportText.RemoveLinesWithoutLetters;
            checkBoxGenerateTimeCodes.Text = Configuration.Settings.Language.ImportText.GenerateTimeCodes;
            checkBoxAutoBreak.Text = Configuration.Settings.Language.Settings.MainTextBoxAutoBreak;
            labelGapBetweenSubtitles.Text = Configuration.Settings.Language.ImportText.GapBetweenSubtitles;
            numericUpDownGapBetweenLines.Left = labelGapBetweenSubtitles.Left + labelGapBetweenSubtitles.Width + 3;
            groupBoxDuration.Text = Configuration.Settings.Language.General.Duration;
            radioButtonDurationAuto.Text = Configuration.Settings.Language.ImportText.Auto;
            radioButtonDurationFixed.Text = Configuration.Settings.Language.ImportText.Fixed;
            buttonRefresh.Text = Configuration.Settings.Language.ImportText.Refresh;
            groupBoxTimeCodes.Text = Configuration.Settings.Language.ImportText.TimeCodes;
            groupBoxImportResult.Text = Configuration.Settings.Language.General.Preview;
            clearToolStripMenuItem.Text = Configuration.Settings.Language.DvdSubRip.Clear;
            startNumberingFromToolStripMenuItem.Text = Configuration.Settings.Language.Main.Menu.Tools.StartNumberingFrom;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            SubtitleListview1.InitializeLanguage(Configuration.Settings.Language.General, Configuration.Settings);
            UiUtil.InitializeSubtitleFont(SubtitleListview1);
            SubtitleListview1.AutoSizeAllColumns(this);

            if (string.IsNullOrEmpty(Configuration.Settings.Tools.ImportTextSplitting))
            {
                radioButtonAutoSplit.Checked = true;
            }
            else if (Configuration.Settings.Tools.ImportTextSplitting.Equals("blank lines", StringComparison.OrdinalIgnoreCase))
            {
                radioButtonSplitAtBlankLines.Checked = true;
            }
            else if (Configuration.Settings.Tools.ImportTextSplitting.Equals("line", StringComparison.OrdinalIgnoreCase))
            {
                radioButtonLineMode.Checked = true;
            }

            comboBoxLineBreak.Text = Configuration.Settings.Tools.ImportTextLineBreak;
            checkBoxMergeShortLines.Checked = Configuration.Settings.Tools.ImportTextMergeShortLines;
            checkBoxRemoveEmptyLines.Checked = Configuration.Settings.Tools.ImportTextRemoveEmptyLines;
            checkBoxAutoSplitAtBlankLines.Checked = Configuration.Settings.Tools.ImportTextAutoSplitAtBlank;
            checkBoxRemoveLinesWithoutLetters.Checked = Configuration.Settings.Tools.ImportTextRemoveLinesNoLetters;
            checkBoxAutoSplitRemoveLinesNoLetters.Checked = Configuration.Settings.Tools.ImportTextRemoveLinesNoLetters;
            checkBoxGenerateTimeCodes.Checked = Configuration.Settings.Tools.ImportTextGenerateTimeCodes;
            checkBoxAutoBreak.Checked = Configuration.Settings.Tools.ImportTextAutoBreak;
            textBoxAsEnd.Text = Configuration.Settings.Tools.ImportTextAutoBreakAtEndMarkerText.Replace(" ", string.Empty);
            checkBoxAutoSplitAtEnd.Checked = Configuration.Settings.Tools.ImportTextAutoBreakAtEnd;
            checkBoxAutoSplitAtEnd.Text = Configuration.Settings.Language.ImportText.SplitAtEndChars;
            textBoxAsEnd.Left = checkBoxAutoSplitAtEnd.Left + checkBoxAutoSplitAtEnd.Width;
            checkBoxAutoSplitAtBlankLines.Text = Configuration.Settings.Language.ImportText.SplitAtBlankLines;

            groupBoxAutoSplitSettings.Text = Configuration.Settings.Language.Settings.Title;
            labelAutoSplitNumberOfLines.Text = Configuration.Settings.Language.Settings.MaximumLines;
            numericUpDownSubtitleLineMaximumLength.Left = labelSubMaxLen.Left + labelSubMaxLen.Width + 3;
            numericUpDownSubtitleLineMaximumLength.Value = Configuration.Settings.General.SubtitleLineMaximumLength;

            if (Configuration.Settings.Tools.ImportTextAutoSplitNumberOfLines >= numericUpDownAutoSplitMaxLines.Minimum &&
                Configuration.Settings.Tools.ImportTextAutoSplitNumberOfLines <= numericUpDownAutoSplitMaxLines.Maximum)
            {
                numericUpDownAutoSplitMaxLines.Value = Configuration.Settings.Tools.ImportTextAutoSplitNumberOfLines;
            }

            if (Configuration.Settings.Tools.ImportTextGap >= numericUpDownGapBetweenLines.Minimum && Configuration.Settings.Tools.ImportTextGap <= numericUpDownGapBetweenLines.Maximum)
            {
                numericUpDownGapBetweenLines.Value = Configuration.Settings.Tools.ImportTextGap;
            }
            if (Configuration.Settings.Tools.ImportTextDurationAuto)
            {
                radioButtonDurationAuto.Checked = true;
            }
            else
            {
                radioButtonDurationFixed.Checked = true;
            }
            numericUpDownDurationFixed.Enabled = radioButtonDurationFixed.Checked;
            if (Configuration.Settings.Tools.ImportTextFixedDuration >= numericUpDownDurationFixed.Minimum &&
                Configuration.Settings.Tools.ImportTextFixedDuration <= numericUpDownDurationFixed.Maximum)
            {
                numericUpDownDurationFixed.Value = Configuration.Settings.Tools.ImportTextFixedDuration;
            }
            UiUtil.FixLargeFonts(this, buttonOK);
            _refreshTimer.Interval = 400;
            _refreshTimer.Tick += RefreshTimerTick;

            if (fileName != null && File.Exists(fileName))
            {
                if (!LoadSingleFile(fileName, parentForm))
                {
                    _exit = true;
                }
            }
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            _refreshTimer.Stop();
            GeneratePreviewReal();
        }

        private void ButtonOpenTextClick(object sender, EventArgs e)
        {
            Text = Configuration.Settings.Language.ImportText.Title;
            openFileDialog1.Title = buttonOpenText.Text;
            if (checkBoxMultipleFiles.Visible && checkBoxMultipleFiles.Checked)
            {
                openFileDialog1.Filter = Configuration.Settings.Language.ImportText.TextFiles + "|*.txt|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            }
            else
            {
                openFileDialog1.Filter = Configuration.Settings.Language.ImportText.TextFiles + "|*.txt;*.rtf;*.tx3g;*.astx;*" + new FinalDraftTemplate2().Extension + "|Adobe Story|*.astx|Final Draft Template|*" + new FinalDraftTemplate2().Extension + "|" + Configuration.Settings.Language.General.AllFiles + "|*.*";
            }

            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Multiselect = checkBoxMultipleFiles.Visible && checkBoxMultipleFiles.Checked;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _startFromNumber = 1;
                if (checkBoxMultipleFiles.Visible && checkBoxMultipleFiles.Checked)
                {
                    foreach (string fileName in openFileDialog1.FileNames)
                    {
                        AddInputFile(fileName);
                    }
                }
                else
                {
                    LoadSingleFile(openFileDialog1.FileName, this);
                }
                GeneratePreview();
            }
        }

        private bool LoadSingleFile(string fileName, Form parentForm)
        {
            groupBoxSplitting.Enabled = true;
            textBoxText.Enabled = true;
            Format = null;
            string ext = string.Empty;
            var extension = Path.GetExtension(fileName);
            if (extension != null)
            {
                ext = extension.ToLowerInvariant();
            }

            var fd = new FinalDraftTemplate2();
            var list = new List<string>(FileUtil.ReadAllLinesShared(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName)));
            bool isFinalDraft = fd.IsMine(list, fileName);

            if (ext == ".astx")
            {
                LoadAdobeStory(fileName);
            }
            else if (isFinalDraft)
            {
                return LoadFinalDraftTemplate(fileName, parentForm ?? this);
            }
            else if (ext == ".tx3g" && new Tx3GTextOnly().IsMine(null, fileName))
            {
                LoadTx3G(fileName);
            }
            else if (ext == ".rtf" && FileUtil.IsRtf(fileName))
            {
                LoadRtf(fileName);
            }
            else
            {
                LoadTextFile(fileName);
            }
            return true;
        }

        private void GeneratePreview()
        {
            if (radioButtonSplitAtBlankLines.Checked || radioButtonLineMode.Checked)
            {
                groupBoxAutoSplitSettings.Visible = false;
                groupBoxAutoSplitSettings.SendToBack();
                checkBoxMergeShortLines.Enabled = true;
                checkBoxRemoveEmptyLines.Enabled = true;
                checkBoxAutoBreak.Enabled = true;
                checkBoxAutoBreak.Text = Configuration.Settings.Language.Settings.MainTextBoxAutoBreak;
            }
            else // autosplit
            {
                groupBoxAutoSplitSettings.Visible = true;
                groupBoxAutoSplitSettings.BringToFront();
                checkBoxMergeShortLines.Enabled = false;
                checkBoxRemoveEmptyLines.Enabled = false;
                checkBoxAutoBreak.Enabled = true;
            }

            if (_refreshTimer.Enabled)
            {
                _refreshTimer.Stop();
            }

            _refreshTimer.Start();
        }

        private void GeneratePreviewReal()
        {
            if (Format == null || Format.GetType() != typeof(CsvNuendo))
            {
                _subtitle = new Subtitle();
                if (checkBoxMultipleFiles.Visible && checkBoxMultipleFiles.Checked)
                {
                    ImportMultipleFiles(listViewInputFiles.Items);
                }
                else if (radioButtonLineMode.Checked)
                {
                    ImportLineMode(textBoxText.Lines);
                }
                else if (radioButtonAutoSplit.Checked)
                {
                    ImportAutoSplit(textBoxText.Lines);
                }
                else
                {
                    ImportSplitAtBlankLine(textBoxText.Lines);
                }
            }
            else
            {
                _subtitle = new Subtitle(_subtitleInput);
                if (checkBoxAutoBreak.Enabled && checkBoxAutoBreak.Checked)
                {
                    foreach (Paragraph p in _subtitle.Paragraphs)
                    {
                        p.Text = Utilities.AutoBreakLine(p.Text);
                    }
                }
            }

            if (checkBoxMergeShortLines.Checked)
            {
                MergeLinesWithContinuation();
            }

            _subtitle.Renumber(_startFromNumber);
            if (checkBoxGenerateTimeCodes.Checked)
            {
                FixDurations();
                MakePseudoStartTime();
            }
            else
            {
                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    p.StartTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                    p.EndTime.TotalMilliseconds = TimeCode.MaxTimeTotalMilliseconds;
                }
            }

            groupBoxImportResult.Text = string.Format(Configuration.Settings.Language.ImportText.PreviewLinesModifiedX, _subtitle.Paragraphs.Count);
            SubtitleListview1.Fill(_subtitle);
            SubtitleListview1.SelectIndexAndEnsureVisible(0);
        }

        private void ImportMultipleFiles(ListView.ListViewItemCollection listViewItemCollection)
        {
            foreach (ListViewItem item in listViewItemCollection)
            {
                string line;
                try
                {
                    line = GetAllText(item.Text).Trim();
                }
                catch
                {
                    line = string.Empty;
                }

                line = line.Replace("|", Environment.NewLine);
                if (comboBoxLineBreak.Text.Length > 0)
                {
                    foreach (string splitter in comboBoxLineBreak.Text.Split(';'))
                    {
                        var tempSplitter = splitter.Trim();
                        if (tempSplitter.Length > 0)
                        {
                            line = line.Replace(tempSplitter, Environment.NewLine);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!checkBoxRemoveEmptyLines.Checked)
                    {
                        _subtitle.Paragraphs.Add(new Paragraph());
                    }
                }
                else if (!PlainTextImporter.ContainsLetters(line))
                {
                    if (!checkBoxRemoveLinesWithoutLetters.Checked)
                    {
                        _subtitle.Paragraphs.Add(new Paragraph(0, 0, line.Trim()));
                    }
                }
                else
                {
                    _subtitle.Paragraphs.Add(new Paragraph(0, 0, line.Trim()));
                }
            }
        }

        private void MergeLinesWithContinuation()
        {
            var temp = new Subtitle();
            bool skipNext = false;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                if (!skipNext)
                {
                    Paragraph next = _subtitle.GetParagraphOrDefault(i + 1);

                    bool merge = !(p.Text.Contains(Environment.NewLine) || next == null) && Configuration.Settings.General.MaxNumberOfLines > 1;

                    if (merge && (p.Text.TrimEnd().EndsWith('!') || p.Text.TrimEnd().EndsWith('.')))
                    {
                        var st = new StrippableText(p.Text);
                        if (st.StrippedText.Length > 0 && char.IsUpper(st.StrippedText[0]))
                        {
                            merge = false;
                        }
                    }

                    if (merge && (p.Text.Length >= Configuration.Settings.General.SubtitleLineMaximumLength - 5 || next.Text.Length >= Configuration.Settings.General.SubtitleLineMaximumLength - 5))
                    {
                        merge = false;
                    }

                    if (merge)
                    {
                        temp.Paragraphs.Add(new Paragraph { Text = p.Text + Environment.NewLine + next.Text });
                        skipNext = true;
                    }
                    else
                    {
                        temp.Paragraphs.Add(new Paragraph(p));
                    }
                }
                else
                {
                    skipNext = false;
                }
            }
            _subtitle = temp;
        }

        private void MakePseudoStartTime()
        {
            var millisecondsInterval = (double)numericUpDownGapBetweenLines.Value;
            double millisecondsIndex = millisecondsInterval;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                p.EndTime.TotalMilliseconds = millisecondsIndex + p.Duration.TotalMilliseconds;
                p.StartTime.TotalMilliseconds = millisecondsIndex;
                millisecondsIndex += (p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds) + millisecondsInterval;
            }
        }

        private void FixDurations()
        {
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (p.Text.Length == 0)
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 2000;
                }
                else
                {
                    if (radioButtonDurationAuto.Checked)
                    {
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + (Utilities.GetOptimalDisplayMilliseconds(p.Text));
                    }
                    else
                    {
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + ((double)numericUpDownDurationFixed.Value);
                    }
                }
            }
        }

        private void ImportLineMode(IEnumerable<string> lines)
        {
            var item = new List<string>();
            foreach (string loopLine in lines)
            {
                // Replace user line break character with Environment.NewLine.
                string line = loopLine;
                if (comboBoxLineBreak.Text.Length > 0)
                {
                    foreach (string splitter in comboBoxLineBreak.Text.Split(';'))
                    {
                        var tempSplitter = splitter.Trim();
                        if (tempSplitter.Length > 0)
                        {
                            line = line.Replace(tempSplitter, Environment.NewLine);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!checkBoxRemoveEmptyLines.Checked)
                    {
                        item.Add(string.Empty);
                    }
                }
                else if (!PlainTextImporter.ContainsLetters(line))
                {
                    if (!checkBoxRemoveLinesWithoutLetters.Checked)
                    {
                        item.Add(line.Trim());
                    }
                }
                else
                {
                    string text = line.Trim();
                    if (checkBoxAutoBreak.Enabled && checkBoxAutoBreak.Checked && comboBoxLineMode.SelectedIndex == 0)
                    {
                        text = Utilities.AutoBreakLine(text);
                    }

                    item.Add(text);
                }

                if (item.Count >= comboBoxLineMode.SelectedIndex + 1)
                {
                    _subtitle.Paragraphs.Add(new Paragraph { Text = string.Join(Environment.NewLine, item.ToArray()) });
                    item.Clear();
                }
            }
            if (item.Count > 0)
            {
                _subtitle.Paragraphs.Add(new Paragraph { Text = string.Join(Environment.NewLine, item.ToArray()) });
            }

        }

        private void ImportSplitAtBlankLine(IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (sb.Length > 0)
                    {
                        string text = sb.ToString().Trim();
                        if (checkBoxAutoBreak.Enabled && checkBoxAutoBreak.Checked)
                        {
                            text = Utilities.AutoBreakLine(text);
                        }

                        _subtitle.Paragraphs.Add(new Paragraph { Text = text });
                        sb.Clear();
                    }
                }
                else if (!PlainTextImporter.ContainsLetters(line))
                {
                    if (!checkBoxRemoveLinesWithoutLetters.Checked)
                    {
                        sb.AppendLine(line.Trim());
                    }
                }
                else
                {
                    sb.AppendLine(line.Trim());
                }
            }
            if (sb.Length > 0)
            {
                SplitSingle(sb);
            }
        }

        private static bool CanMakeThreeLiner(out string text, string input)
        {
            text = string.Empty;
            if (input.Length < Configuration.Settings.General.SubtitleLineMaximumLength * 3 && input.Length > Configuration.Settings.General.SubtitleLineMaximumLength * 1.5)
            {
                var breaked = Utilities.AutoBreakLine(input).SplitToLines();
                if (breaked.Count == 2 && (breaked[0].Length > Configuration.Settings.General.SubtitleLineMaximumLength || breaked[1].Length > Configuration.Settings.General.SubtitleLineMaximumLength))
                {
                    var first = new StringBuilder();
                    var second = new StringBuilder();
                    var third = new StringBuilder();
                    foreach (string word in input.Replace(Environment.NewLine, " ").Replace("  ", " ").Split(' '))
                    {
                        if (first.Length + word.Length < Configuration.Settings.General.SubtitleLineMaximumLength)
                        {
                            first.Append(' ');
                            first.Append(word);
                        }
                        else if (second.Length + word.Length < Configuration.Settings.General.SubtitleLineMaximumLength)
                        {
                            second.Append(' ');
                            second.Append(word);
                        }
                        else
                        {
                            third.Append(' ');
                            third.Append(word);
                        }
                    }
                    if (third.Length <= Configuration.Settings.General.SubtitleLineMaximumLength && third.Length > 10)
                    {
                        if (second.Length > 15)
                        {
                            string ending = second.ToString().Substring(second.Length - 9);
                            int splitPos = -1;
                            if (ending.Contains(". "))
                            {
                                splitPos = ending.IndexOf(". ", StringComparison.Ordinal) + second.Length - 9;
                            }
                            else if (ending.Contains("! "))
                            {
                                splitPos = ending.IndexOf("! ", StringComparison.Ordinal) + second.Length - 9;
                            }
                            else if (ending.Contains(", "))
                            {
                                splitPos = ending.IndexOf(", ", StringComparison.Ordinal) + second.Length - 9;
                            }
                            else if (ending.Contains("? "))
                            {
                                splitPos = ending.IndexOf("? ", StringComparison.Ordinal) + second.Length - 9;
                            }

                            if (splitPos > 0)
                            {
                                text = Utilities.AutoBreakLine(first.ToString().Trim() + second.ToString().Substring(0, splitPos + 1)).Trim() + Environment.NewLine + (second.ToString().Substring(splitPos + 1) + third).Trim();
                                return true;
                            }
                        }

                        text = first + Environment.NewLine + second + Environment.NewLine + third;
                        return true;
                    }
                }
            }
            return false;
        }

        private void SplitSingle(StringBuilder sb)
        {
            string t = sb.ToString().Trim();
            var tarr = t.SplitToLines();
            if (checkBoxMergeShortLines.Checked == false && tarr.Count == 3 &&
                tarr[0].Length < Configuration.Settings.General.SubtitleLineMaximumLength &&
                tarr[1].Length < Configuration.Settings.General.SubtitleLineMaximumLength &&
                tarr[2].Length < Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                _subtitle.Paragraphs.Add(new Paragraph { Text = tarr[0] + Environment.NewLine + tarr[1] });
                return;
            }
            if (checkBoxMergeShortLines.Checked == false && tarr.Count == 2 &&
                tarr[0].Length < Configuration.Settings.General.SubtitleLineMaximumLength &&
                tarr[1].Length < Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                _subtitle.Paragraphs.Add(new Paragraph { Text = tarr[0] + Environment.NewLine + tarr[1] });
                return;
            }
            if (checkBoxMergeShortLines.Checked == false && tarr.Count == 1 && tarr[0].Length < Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                _subtitle.Paragraphs.Add(new Paragraph { Text = tarr[0].Trim() });
                return;
            }

            Paragraph p = null;
            if (CanMakeThreeLiner(out var threeliner, sb.ToString()))
            {
                var parts = threeliner.SplitToLines();
                _subtitle.Paragraphs.Add(new Paragraph { Text = parts[0] + Environment.NewLine + parts[1] });
                _subtitle.Paragraphs.Add(new Paragraph { Text = parts[2].Trim() });
                return;
            }

            foreach (string text in Utilities.AutoBreakLineMoreThanTwoLines(sb.ToString(), Configuration.Settings.General.SubtitleLineMaximumLength, Configuration.Settings.General.MergeLinesShorterThan, "en").SplitToLines())
            {
                if (p == null)
                {
                    p = new Paragraph { Text = text };
                }
                else if (p.Text.Contains(Environment.NewLine))
                {
                    _subtitle.Paragraphs.Add(p);
                    p = new Paragraph();
                    if (text.Length >= Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        p.Text = Utilities.AutoBreakLine(text);
                    }
                    else
                    {
                        p.Text = text;
                    }
                }
                else
                {
                    if (checkBoxMergeShortLines.Checked || p.Text.Length > Configuration.Settings.General.SubtitleLineMaximumLength || text.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        p.Text = Utilities.AutoBreakLine(p.Text + Environment.NewLine + text.Trim());
                    }
                    else
                    {
                        p.Text = p.Text + Environment.NewLine + text.Trim();
                    }
                }
            }
            if (p != null)
            {
                _subtitle.Paragraphs.Add(p);
            }
        }

        private void ImportAutoSplit(string[] textLines)
        {
            var sub = new Subtitle();
            foreach (var line in textLines)
            {
                sub.Paragraphs.Add(new Paragraph(line, 0, 0));
            }
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(sub);

            var plainTextImporter = new PlainTextImporter(checkBoxAutoSplitAtBlankLines.Checked,
                checkBoxAutoSplitRemoveLinesNoLetters.Checked,
                (int)numericUpDownAutoSplitMaxLines.Value,
                checkBoxAutoSplitAtEnd.Checked ? textBoxAsEnd.Text : string.Empty,
                (int)numericUpDownSubtitleLineMaximumLength.Value,
                language);

            ImportLineMode(plainTextImporter.ImportAutoSplit(textLines));
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = SubtitleListview1.Items.Count > 0 ? DialogResult.OK : DialogResult.Cancel;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void CheckBoxRemoveLinesWithoutLettersOrNumbersCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void CheckBoxRemoveEmptyLinesCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void RadioButtonLineModeCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
            // textBoxLineBreak and its label are enabled if radioButtonLineMode is checked.
            comboBoxLineBreak.Enabled = radioButtonLineMode.Checked;
            labelLineBreak.Enabled = radioButtonLineMode.Checked;
        }

        private void RadioButtonAutoSplitCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void TextBoxTextDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void TextBoxTextDragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                LoadTextFile(files[0]);
            }
        }

        private static string HtmlToPlainText(string html)
        {
            var stripFormattingRegex = new Regex(@"<[^>]*(>|$)", RegexOptions.Multiline); // match any character between '<' and '>', even when end tag is missing
            var tagWhiteSpaceRegex = new Regex(@"(>|$)(\W|\n|\r)+<", RegexOptions.Multiline); // matches one or more (white space or line breaks) between '>' and '<'

            // Decode html specific characters
            var text = System.Net.WebUtility.HtmlDecode(html);

            // Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");

            // Find new lines
            text = text.Replace("<BR>", Environment.NewLine);
            text = text.Replace("<br>", Environment.NewLine);
            text = text.Replace("<br />", Environment.NewLine);
            text = text.Replace("<br/>", Environment.NewLine);
            text = text.Replace("<HR>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("<hr>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("<hr />", Environment.NewLine + Environment.NewLine);
            text = text.Replace("<hr/>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("</p>", Environment.NewLine + Environment.NewLine);
            text = text.Replace("</P>", Environment.NewLine + Environment.NewLine);
            text = text.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine);
            text = text.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine);

            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }

        private static string GetAllText(string fileName)
        {
            try
            {
                Encoding encoding = LanguageAutoDetect.GetEncodingFromFile(fileName);
                var text = FileUtil.ReadAllTextShared(fileName, encoding);
                if (fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
                {
                    text = HtmlToPlainText(text);
                }

                return text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return string.Empty;
            }
        }

        private void LoadTextFile(string fileName)
        {
            textBoxText.Text = GetAllText(fileName);
            SetVideoFileName(fileName);
            Text = Configuration.Settings.Language.ImportText.Title + " - " + fileName;
            GeneratePreview();
        }

        private void LoadTx3G(string fileName)
        {
            try
            {
                var sb = new StringBuilder();
                var sub = new Subtitle();
                new Tx3GTextOnly().LoadSubtitle(sub, null, fileName);
                foreach (var paragraph in sub.Paragraphs)
                {
                    sb.AppendLine(paragraph.Text);
                    sb.AppendLine();
                }
                textBoxText.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            radioButtonSplitAtBlankLines.Checked = true;
            checkBoxMergeShortLines.Checked = false;
            GeneratePreview();
        }

        private void LoadRtf(string fileName)
        {
            const int bomHeaderLength = 3;
            var encoding = FileUtil.HasUtf8Bom(fileName) ? Encoding.UTF8 : Encoding.ASCII;
            var bytes = FileUtil.ReadAllBytesShared(fileName);
            var rtf = encoding.GetString(bytes);
            if (Equals(encoding, Encoding.UTF8))
            {
                rtf = encoding.GetString(bytes, bomHeaderLength, bytes.Length - bomHeaderLength);
            }
            using (var rtb = new RichTextBox { Rtf = rtf })
            {
                textBoxText.Text = string.Join(Environment.NewLine, rtb.Text.SplitToLines());
            }
            SetVideoFileName(fileName);
            Text = Configuration.Settings.Language.ImportText.Title + " - " + fileName;
            GeneratePreview();
        }


        private void LoadAdobeStory(string fileName)
        {
            try
            {
                var sb = new StringBuilder();
                var doc = new XmlDocument();
                doc.Load(fileName);
                var nodes = doc.DocumentElement?.SelectNodes("//paragraph[@element='Dialog']");
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes) // <paragraph objID="1:28" element="Dialog">
                    {
                        XmlNode textRun = node.SelectSingleNode("textRun"); // <textRun objID="1:259">Yeah...I suppose</textRun>
                        if (textRun != null)
                        {
                            sb.AppendLine(textRun.InnerText);
                        }
                    }
                }

                textBoxText.Text = sb.ToString();
                _videoFileName = null;
                Text = Configuration.Settings.Language.ImportText.Title + " - " + fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            radioButtonLineMode.Checked = true;
            checkBoxMergeShortLines.Checked = false;
            GeneratePreview();
        }

        private bool LoadFinalDraftTemplate(string fileName, Form parentForm)
        {
            try
            {
                var fd = new FinalDraftTemplate2();
                var sub = new Subtitle();
                var lines = Encoding.UTF8.GetString(FileUtil.ReadAllBytesShared(fileName)).SplitToLines();
                var availableParagraphTypes = fd.GetParagraphTypes(lines);
                using (var form = new ImportFinalDraft(availableParagraphTypes))
                {
                    if (form.ShowDialog(parentForm) == DialogResult.OK)
                    {
                        fd.ActiveParagraphTypes = form.ChosenParagraphTypes;
                        fd.LoadSubtitle(sub, lines, fileName);
                    }
                    else
                    {
                        return false;
                    }
                }

                textBoxText.Text = sub.ToText(fd);
                _videoFileName = null;
                Text = Configuration.Settings.Language.ImportText.Title + " - " + fileName;
                _subtitleInput = sub;
                Format = new CsvNuendo();
                groupBoxSplitting.Enabled = false;
                textBoxText.Enabled = false;
                if (_subtitleInput.Paragraphs.Any(p => !string.IsNullOrEmpty(p.Actor)))
                {
                    SubtitleListview1.ShowActorColumn(Configuration.Settings.Language.General.Character);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            radioButtonLineMode.Checked = true;
            checkBoxMergeShortLines.Checked = false;
            GeneratePreview();
            return true;
        }

        private void SetVideoFileName(string fileName)
        {
            var videoExtensions = new[] { ".mkv", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mpg" };
            _videoFileName = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length);
            if (_videoFileName.EndsWith(".en", StringComparison.Ordinal))
            {
                _videoFileName = _videoFileName.Remove(_videoFileName.Length - 3);
            }

            foreach (var ext in videoExtensions)
            {
                if (File.Exists(_videoFileName + ext))
                {
                    _videoFileName += ext;
                    return;
                }
            }

            var dir = Path.GetDirectoryName(fileName);
            if (dir != null)
            {
                foreach (var ext in videoExtensions)
                {
                    var files = Directory.GetFiles(dir, Path.GetFileNameWithoutExtension(_videoFileName) + "*" + ext);
                    if (files.Length > 0)
                    {
                        _videoFileName = files[0];
                        return;
                    }
                }
                _videoFileName = null;
            }
        }

        private void ButtonRefreshClick(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void RadioButtonDurationFixedCheckedChanged(object sender, EventArgs e)
        {
            numericUpDownDurationFixed.Enabled = radioButtonDurationFixed.Checked;
            GeneratePreview();
        }

        private void CheckBoxMergeShortLinesCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void ImportTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void TextBoxTextTextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void NumericUpDownDurationFixedValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void NumericUpDownGapBetweenLinesValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void RadioButtonDurationAutoCheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void radioButtonSplitAtBlankLines_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void checkBoxGenerateTimeCodes_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxTimeCodes.Enabled = checkBoxGenerateTimeCodes.Checked;
            GeneratePreview();
        }

        private void ImportText_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (radioButtonSplitAtBlankLines.Checked)
            {
                Configuration.Settings.Tools.ImportTextSplitting = "blank lines";
            }
            else if (radioButtonLineMode.Checked)
            {
                Configuration.Settings.Tools.ImportTextSplitting = "line";
            }
            else
            {
                Configuration.Settings.Tools.ImportTextSplitting = "auto";
            }

            Configuration.Settings.Tools.ImportTextLineBreak = comboBoxLineBreak.Text.Trim();
            Configuration.Settings.Tools.ImportTextMergeShortLines = checkBoxMergeShortLines.Checked;
            Configuration.Settings.Tools.ImportTextRemoveEmptyLines = checkBoxRemoveEmptyLines.Checked;
            Configuration.Settings.Tools.ImportTextAutoSplitNumberOfLines = numericUpDownAutoSplitMaxLines.Value;
            Configuration.Settings.Tools.ImportTextAutoSplitAtBlank = checkBoxAutoSplitAtBlankLines.Checked;

            if (radioButtonAutoSplit.Checked)
            {
                Configuration.Settings.Tools.ImportTextRemoveLinesNoLetters = checkBoxAutoSplitRemoveLinesNoLetters.Checked;
            }
            else
            {
                Configuration.Settings.Tools.ImportTextRemoveLinesNoLetters = checkBoxRemoveLinesWithoutLetters.Checked;
            }

            Configuration.Settings.Tools.ImportTextGenerateTimeCodes = checkBoxGenerateTimeCodes.Checked;
            Configuration.Settings.Tools.ImportTextAutoBreak = checkBoxAutoBreak.Checked;
            Configuration.Settings.Tools.ImportTextAutoBreakAtEnd = checkBoxAutoSplitAtEnd.Checked;
            Configuration.Settings.Tools.ImportTextAutoBreakAtEndMarkerText = textBoxAsEnd.Text.Replace(" ", string.Empty);
            Configuration.Settings.Tools.ImportTextGap = numericUpDownGapBetweenLines.Value;
            Configuration.Settings.Tools.ImportTextDurationAuto = radioButtonDurationAuto.Checked;
            Configuration.Settings.Tools.ImportTextFixedDuration = numericUpDownDurationFixed.Value;
        }

        private void checkBoxMultipleFiles_CheckedChanged(object sender, EventArgs e)
        {
            Format = null;
            textBoxText.Enabled = true;
            groupBoxSplitting.Enabled = true;
            if (checkBoxMultipleFiles.Checked)
            {
                listViewInputFiles.Visible = true;
                textBoxText.Visible = false;
                buttonOpenText.Text = Configuration.Settings.Language.ImportText.OpenTextFiles;
                groupBoxSplitting.Enabled = false;
            }
            else
            {
                listViewInputFiles.Visible = false;
                textBoxText.Visible = true;
                buttonOpenText.Text = Configuration.Settings.Language.ImportText.OpenTextFile;
                groupBoxSplitting.Enabled = true;
            }
            GeneratePreview();
        }

        private void listViewInputFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listViewInputFiles_DragDrop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string fileName in fileNames.OrderBy(p => p))
            {
                AddInputFile(fileName);
            }
            GeneratePreview();
        }

        private void AddInputFile(string fileName)
        {
            try
            {
                var fi = new FileInfo(fileName);
                var item = new ListViewItem(fileName) { Tag = fileName };
                item.SubItems.Add(Utilities.FormatBytesToDisplayFileSize(fi.Length));
                if (fi.Length < 1024 * 1024) // max 1 mb
                {
                    listViewInputFiles.Items.Add(item);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void checkBoxAutoBreak_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void comboBoxLineBreak_TextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listViewInputFiles.Items.Clear();
        }

        private void ImportText_Shown(object sender, EventArgs e)
        {
            if (textBoxText.Visible && textBoxText.Text.Length > 20)
            {
                buttonOK.Focus();
            }

            if (_exit)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void startNumberingFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var startNumberingFrom = new StartNumberingFrom())
            {
                if (startNumberingFrom.ShowDialog(this) == DialogResult.OK)
                {
                    _startFromNumber = startNumberingFrom.StartFromNumber;
                    _subtitle.Renumber(startNumberingFrom.StartFromNumber);
                    SubtitleListview1.Fill(_subtitle);
                }
            }
        }

        private void numericUpDownAutoSplitMaxLines_ValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void textBoxAsEnd1_TextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void checkBoxAutoSplitAtBlankLines_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void checkBoxAutoSplitRemoveLinesNoLetters_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void textBoxAsEnd1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Space || char.IsLetterOrDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void numericUpDownSubtitleLineMaximumLength_ValueChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void comboBoxLineMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void checkBoxAutoSplitAtEnd_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }
    }
}