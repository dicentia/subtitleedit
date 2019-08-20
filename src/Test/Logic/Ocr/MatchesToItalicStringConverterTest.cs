﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic.Ocr;
using Nikse.SubtitleEdit.Forms.Ocr;

namespace Test.Logic.Ocr
{
    [TestClass]
    public class MatchesToItalicStringConverterTest
    {

        [TestMethod]
        public void NormalItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("<i>Hallo!</i>", result);
        }

        [TestMethod]
        public void NormalNonItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("l", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", false, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Hallo!", result);
        }

        [TestMethod]
        public void TestQuotesInItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("d", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(":", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("G", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("w", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("'", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("He said: <i>''Go now!''</i>", result);
        }

        [TestMethod]
        public void TestWordInItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("d", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("w", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("He said <i>now!</i>", result);
        }

        [TestMethod]
        public void TestPartInItalic()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("H", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch(" ", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("A", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("g", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("-", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Hi Angi-<i>san!</i>", result);
        }

        [TestMethod]
        public void TestPartInItalicWithDash()
        {
            var matches = new List<VobSubOcr.CompareMatch>
            {
                new VobSubOcr.CompareMatch("I", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("w", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("m", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("t", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("o", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("-", false, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("e", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("n", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("s", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("a", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("i", true, 0, Guid.NewGuid().ToString()),
                new VobSubOcr.CompareMatch("!", true, 0, Guid.NewGuid().ToString()),
            };

            string result = MatchesToItalicStringConverter.GetStringWithItalicTags(matches);
            Assert.AreEqual("Iwamoto-<i>sensai!</i>", result);
        }

    }
}
