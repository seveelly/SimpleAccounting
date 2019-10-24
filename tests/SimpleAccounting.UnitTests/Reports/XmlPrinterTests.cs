﻿// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace SimpleAccounting.UnitTests.Reports
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Printing;
    using System.Xml.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using lg2de.SimpleAccounting.Reports;
    using Xunit;

    public class XmlPrinterTests
    {
        private static readonly List<PaperSize> PaperSizes = new List<PaperSize> { new PaperSize("A4", (int)(210 / 0.254), (int)(297 / 0.254)) };

        [Fact]
        public void LoadDocument_UnknownReport_Throws()
        {
            var sut = new XmlPrinter();

            sut.Invoking(x => x.LoadDocument("XXX")).Should().Throw<ArgumentException>()
                .WithMessage("*XXX*");
        }

        [Fact]
        public void LoadDocument_AccountJournalReport_Loaded()
        {
            var sut = new XmlPrinter();

            sut.Invoking(x => x.LoadDocument(AccountJournalReport.ResourceName)).Should().NotThrow();
        }

        [Fact]
        public void LoadDocument_AnnualBalanceReport_Loaded()
        {
            var sut = new XmlPrinter();

            sut.Invoking(x => x.LoadDocument(AnnualBalanceReport.ResourceName)).Should().NotThrow();
        }

        [Fact]
        public void LoadDocument_TotalJournalReport_Loaded()
        {
            var sut = new XmlPrinter();

            sut.Invoking(x => x.LoadDocument(TotalJournalReport.ResourceName)).Should().NotThrow();
        }

        [Fact]
        public void LoadDocument_TotalsAndBalancesReport_Loaded()
        {
            var sut = new XmlPrinter();

            sut.Invoking(x => x.LoadDocument(TotalsAndBalancesReport.ResourceName)).Should().NotThrow();
        }

        [Fact]
        public void SetupDocument_A4_DocumentInitialized()
        {
            var sut = new XmlPrinter();
            var document = new PrintDocument();
            sut.LoadXml("<root paperSize=\"A4\" />");

            sut.SetupDocument(document, PaperSizes);

            using (new AssertionScope())
            {
                sut.DocumentWidth.Should().Be(210);
                sut.DocumentHeight.Should().Be(297);
            }
        }

        [Fact]
        public void SetupDocument_A4Landscape_DocumentInitialized()
        {
            var sut = new XmlPrinter();
            var document = new PrintDocument();
            sut.LoadXml("<root paperSize=\"A4\" landscape=\"true\" />");

            sut.SetupDocument(document, PaperSizes);

            using (new AssertionScope())
            {
                sut.DocumentWidth.Should().Be(297);
                sut.DocumentHeight.Should().Be(210);
            }
        }

        [Fact]
        public void SetupDocument_Custom_DocumentInitialized()
        {
            var sut = new XmlPrinter();
            var document = new PrintDocument();
            sut.LoadXml("<root paperSize=\"custom\" width=\"10\" height=\"20\" />");

            sut.SetupDocument(document, PaperSizes);

            using (new AssertionScope())
            {
                sut.DocumentWidth.Should().Be(10);
                sut.DocumentHeight.Should().Be(20);
            }
        }

        [Fact]
        public void SetupDocument_Margins_DocumentInitialized()
        {
            var sut = new XmlPrinter();
            var document = new PrintDocument();
            sut.LoadXml("<root left=\"1\" top=\"2\" bottom=\"3\" />");

            sut.SetupDocument(document, PaperSizes);

            using (new AssertionScope())
            {
                sut.DocumentLeftMargin.Should().Be(1);
                sut.DocumentTopMargin.Should().Be(2);
                sut.DocumentBottomMargin.Should().Be(3);
            }
        }

        [Fact]
        public void TransformDocument_Rectangle_ConvertedToLines()
        {
            var sut = new XmlPrinter();
            sut.LoadXml("<root><rectangle relFromX=\"10\" relFromY=\"20\" relToX=\"30\" relToY=\"40\" /></root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                "<root>"
                + "<line relFromX=\"10\" relFromY=\"20\" relToX=\"30\" relToY=\"20\" />"
                + "<line relFromX=\"30\" relFromY=\"20\" relToX=\"30\" relToY=\"40\" />"
                + "<line relFromX=\"30\" relFromY=\"40\" relToX=\"10\" relToY=\"40\" />"
                + "<line relFromX=\"10\" relFromY=\"40\" relToX=\"10\" relToY=\"20\" />"
                + "</root>"));
        }

        [Fact]
        public void TransformDocument_Table_ConvertedToTexts()
        {
            var sut = new XmlPrinter { DocumentHeight = 100 };
            sut.LoadXml(
                "<root>"
                + "<table><columns>"
                + "<column width=\"10\">C1</column>"
                + "<column width=\"20\">C2</column>"
                + "</columns><data>"
                + "<tr><td>1</td><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                "<root>"
                + "<text relX=\"0\">C1</text>"
                + "<text relX=\"10\">C2</text>"
                + "<move relY=\"4\" />" // DefaultLineHeight
                + "<text relX=\"0\">1</text>"
                + "<text relX=\"10\">2</text>"
                + "<move relY=\"4\" />"
                + "</root>"));
        }

        [Fact]
        public void TransformDocument_TableCenterAlign_ConvertedToTexts()
        {
            var sut = new XmlPrinter { DocumentHeight = 100 };
            sut.LoadXml(
                "<root>"
                + "<table><columns>"
                + "<column width=\"10\" align=\"center\">C1</column>"
                + "<column width=\"20\">C2</column>"
                + "</columns><data>"
                + "<tr><td>1</td><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                    "<root>"
                    + "<text relX=\"5\" align=\"center\">C1</text>"
                    + "<text relX=\"10\">C2</text>"
                    + "<move relY=\"4\" />" // DefaultLineHeight
                    + "<text relX=\"5\" align=\"center\">1</text>"
                    + "<text relX=\"10\">2</text>"
                    + "<move relY=\"4\" />"
                    + "</root>"));
        }

        [Fact]
        public void TransformDocument_TableRightAlign_ConvertedToTexts()
        {
            var sut = new XmlPrinter { DocumentHeight = 100 };
            sut.LoadXml(
                "<root>"
                + "<table><columns>"
                + "<column width=\"10\" align=\"right\">C1</column>"
                + "<column width=\"20\">C2</column>"
                + "</columns><data>"
                + "<tr><td>1</td><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                    "<root>"
                    + "<text relX=\"10\" align=\"right\">C1</text>"
                    + "<text relX=\"10\">C2</text>"
                    + "<move relY=\"4\" />" // DefaultLineHeight
                    + "<text relX=\"10\" align=\"right\">1</text>"
                    + "<text relX=\"10\">2</text>"
                    + "<move relY=\"4\" />"
                    + "</root>"));
        }

        [Fact]
        public void TransformDocument_TableLeftLine_ConvertedToTexts()
        {
            var sut = new XmlPrinter { DocumentHeight = 100 };
            sut.LoadXml(
                "<root>"
                + "<table><columns>"
                + "<column width=\"10\" leftLine=\"true\">C1</column>"
                + "<column width=\"20\">C2</column>"
                + "</columns><data>"
                + "<tr><td>1</td><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                    "<root>"
                    + "<line relToY=\"4\" />" // DefaultLineHeight
                    + "<text relX=\"0\">C1</text>"
                    + "<text relX=\"10\">C2</text>"
                    + "<move relY=\"4\" />"
                    + "<line relToY=\"4\" />"
                    + "<text relX=\"0\">1</text>"
                    + "<text relX=\"10\">2</text>"
                    + "<move relY=\"4\" />"
                    + "</root>"));
        }

        [Fact]
        public void TransformDocument_TableRightLine_ConvertedToTexts()
        {
            var sut = new XmlPrinter { DocumentHeight = 100 };
            sut.LoadXml(
                "<root>"
                + "<table><columns>"
                + "<column width=\"10\" rightLine=\"true\">C1</column>"
                + "<column width=\"20\">C2</column>"
                + "</columns><data>"
                + "<tr><td>1</td><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                    "<root>"
                    + "<line relFromX=\"10\" relToX=\"10\" relToY=\"4\" />"
                    + "<text relX=\"0\">C1</text>"
                    + "<text relX=\"10\">C2</text>"
                    + "<move relY=\"4\" />"
                    + "<line relFromX=\"10\" relToX=\"10\" relToY=\"4\" />"
                    + "<text relX=\"0\">1</text>"
                    + "<text relX=\"10\">2</text>"
                    + "<move relY=\"4\" />"
                    + "</root>"));
        }

        [Fact]
        public void TransformDocument_TableTopLine_ConvertedToTexts()
        {
            var sut = new XmlPrinter { DocumentHeight = 100 };
            sut.LoadXml(
                "<root>"
                + "<table><columns>"
                + "<column width=\"10\" topLine=\"true\">C1</column>"
                + "<column width=\"20\">C2</column>"
                + "</columns><data>"
                + "<tr><td>1</td><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                    "<root>"
                    + "<line relToX=\"10\" />"
                    + "<text relX=\"0\">C1</text>"
                    + "<text relX=\"10\">C2</text>"
                    + "<move relY=\"4\" />"
                    + "<line relToX=\"10\" />"
                    + "<text relX=\"0\">1</text>"
                    + "<text relX=\"10\">2</text>"
                    + "<move relY=\"4\" />"
                    + "</root>"));
        }

        [Fact]
        public void TransformDocument_TableBottomLine_ConvertedToTexts()
        {
            var sut = new XmlPrinter { DocumentHeight = 100 };
            sut.LoadXml(
                "<root>"
                + "<table><columns>"
                + "<column width=\"10\" bottomLine=\"true\">C1</column>"
                + "<column width=\"20\">C2</column>"
                + "</columns><data>"
                + "<tr><td>1</td><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                    "<root>"
                    + "<line relToX=\"10\" relFromY=\"4\" relToY=\"4\" />"
                    + "<text relX=\"0\">C1</text>"
                    + "<text relX=\"10\">C2</text>"
                    + "<move relY=\"4\" />"
                    + "<line relToX=\"10\" relFromY=\"4\" relToY=\"4\" />"
                    + "<text relX=\"0\">1</text>"
                    + "<text relX=\"10\">2</text>"
                    + "<move relY=\"4\" />"
                    + "</root>"));
        }

        [Fact]
        public void TransformDocument_LongTable_NewPageWithHeader()
        {
            var sut = new XmlPrinter { DocumentHeight = 10 };
            sut.LoadXml(
                "<root>"
                + "<table><columns>"
                + "<column width=\"10\">C1</column>"
                + "</columns><data>"
                + "<tr><td>1</td></tr>"
                + "<tr><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                "<root>"
                + "<text relX=\"0\">C1</text>"
                + "<move relY=\"4\" />"
                + "<text relX=\"0\">1</text>"
                + "<newpage />"
                + "<text relX=\"0\">C1</text>"
                + "<move relY=\"4\" />"
                + "<text relX=\"0\">2</text>"
                + "<move relY=\"4\" />"
                + "</root>"));
        }

        [Fact]
        public void TransformDocument_PageTexts_ConvertedToLines()
        {
            var sut = new XmlPrinter { DocumentHeight = 10 };
            sut.LoadXml(
                "<root>"
                + "<pageTexts><font><text>page {page}</text></font></pageTexts>"
                + "<table><columns>"
                + "<column width=\"10\">C1</column>"
                + "</columns><data>"
                + "<tr><td>1</td></tr>"
                + "<tr><td>2</td></tr>"
                + "</data></table>"
                + "</root>");

            sut.TransformDocument();

            XDocument.Parse(sut.Document.OuterXml).Should().BeEquivalentTo(
                XDocument.Parse(
                "<root>"
                + "<font><text>page 1</text></font>"
                + "<text relX=\"0\">C1</text>"
                + "<move relY=\"4\" />"
                + "<text relX=\"0\">1</text>"
                + "<newpage />"
                + "<font><text>page 2</text></font>"
                + "<text relX=\"0\">C1</text>"
                + "<move relY=\"4\" />"
                + "<text relX=\"0\">2</text>"
                + "<move relY=\"4\" />"
                + "</root>"));
        }
    }
}
