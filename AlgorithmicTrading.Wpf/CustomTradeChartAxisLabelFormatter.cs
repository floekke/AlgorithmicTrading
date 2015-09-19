//using Abt.Controls.SciChart.Visuals.Axes;
//using System;

//namespace AlgorithmicTrading.Wpf
//{
//    public class CustomTradeChartAxisLabelFormatter : TradeChartAxisLabelFormatter
//    {
//        public override string FormatCursorLabel(IComparable dataValue)
//        {
//            if (dataValue == null)
//            {
//                return "N/A";
//            }

//            var date = Convert.ToDateTime(dataValue);

//            if (date.Year == 1)
//            {
//                return "N/A";
//            }

//            return base.FormatCursorLabel(dataValue);
//        }

//        public override string FormatLabel(IComparable dataValue)
//        {
//            if (dataValue == null)
//            {
//                return "N/A";
//            }

//            var date = Convert.ToDateTime(dataValue);

//            if (date.Year == 1)
//            {
//                return "N/A";
//            }

//            return base.FormatLabel(dataValue);
//        }
//    }
//}
