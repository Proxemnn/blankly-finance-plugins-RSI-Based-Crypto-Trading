using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;

namespace Rigel
{
    public partial class Form1 : Form
    {
        private FTXClient ss;
        List<string> selectedContracts;
        System.Threading.Timer tt, tt2;
        bool updateHistoricalData;

        public Form1()
        {
            InitializeComponent();

            chart1.ChartAreas[0].InnerPlotPosition.Auto = true;
            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart1.Legends[0].Docking = Docking.Top;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "dd-MMM HH:mm:ss";
            chart1.ChartAreas[0].AxisX.LabelStyle.Angle = 45;


            chart2.ChartAreas[0].InnerPlotPosition.Auto = true;
            chart2.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart2.Legends[0].Docking = Docking.Top;
            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "dd-MMM HH:mm:ss";
            chart2.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
            chart2.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chart2.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chart2.Titles.Add("Implied Yields (% pa)");

            chart3.ChartAreas[0].InnerPlotPosition.Auto = true;
            chart3.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart3.Legends[0].Enabled = false;
            chart3.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
            chart3.Titles.Add("Implied Yield Curve & Forward Rates (% pa)");

            chart4.ChartAreas[0].InnerPlotPosition.Auto = true;
            chart4.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart4.Legends[0].Enabled = false;
            chart4.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
            chart4.ChartAreas[0].AxisX.Interval = 1;
            chart4.Titles.Add("Highest Yielding Tokens (% pa)");

            chart5.ChartAreas[0].InnerPlotPosition.Auto = true;
            chart5.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart5.Legends[0].Docking = Docking.Top;
            chart5.ChartAreas[0].AxisX.LabelStyle.Format = "dd-MMM HH:mm:ss";
            chart5.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
            chart5.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chart5.ChartAreas[0].AxisY2.IsStartedFromZero = false;
            chart5.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
            chart5.Titles.Add("Accrued interest on Perpetual Future (% pa)");


            selectedContracts = new List<string>();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ss = new FTXClient(textBox1.Text, textBox2.Text);
            await ss.Initialize();

            comboBox1.DataSource = ss.tokens;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                listBox1.DataSource = ss.futures.Where(x => x.Value.underlying == comboBox1.Text).Select(x => x.Value.name).ToList();
                this.Text = "Rigel : " + comboBox1.Text;

                updateHistoricalData = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (tt != null)
                tt.Dispose();

            selectedContracts = ((ListBox)sender).SelectedItems.Cast<string>().ToList();
            selectedContracts = selectedContracts.Prepend(comboBox1.Text + "/USD").ToList();

            chart1.Series.Clear();
            chart2.Series.Clear();
            chart3.Series.Clear();
            chart4.Series.Clear();
            chart5.Series.Clear();

            updateHistoricalData = true;

            tt = new System.Threading.Timer(new System.Threading.TimerCallback(UpdatePriceCharts), null, 0, int.Parse(textBox4.Text) * 1000);
            tt2 = new System.Threading.Timer(new System.Threading.TimerCallback(UpdateFundingRates), null, 0, int.Parse(textBox4.Text) * 1000);
        }

        private async void UpdatePriceCharts(object state)
        {
            Tuple<DateTime, Market[]> markets = await ss.GetMultipleMarketsAsync(selectedContracts);

            List<Tuple<DateTime, double>[]> historicalPrices = null;
            List<Tuple<DateTime, double>[]> historicalFundingRates = null;
            if (updateHistoricalData)
            {
                historicalPrices = await ss.GetMultipleHistoricalMarketsAsync(selectedContracts, 60 * 60, DateTime.Now.AddDays(-int.Parse(textBox3.Text)), DateTime.Now);
                if (selectedContracts.Count == historicalPrices.Count)
                    historicalFundingRates = await ss.GetHistoricalFundingRatesAsync(selectedContracts, historicalPrices);
            }

            // Just in case changes have been made while retrieving markets.
            if (markets.Item2.Length != selectedContracts.Count)
                return;

            label4.Invoke(new MethodInvoker(delegate
            {
                label4.Text = "";
            }));

            chart3.Invoke(new MethodInvoker(delegate
            {
                foreach (var s in chart3.Series)
                    s.Points.Clear();
            }));

            int i = 0;
            double prevYF = 0, prevRate = 0;
            List<Tuple<string, DateTime, double, bool>> dataForChart1 = new List<Tuple<string, DateTime, double, bool>>();
            List<Tuple<string, DateTime, double, bool>> dataForChart2 = new List<Tuple<string, DateTime, double, bool>>();
            List<Tuple<string, string, double, bool>> dataForChart3_implied = new List<Tuple<string, string, double, bool>>();
            List<Tuple<string, string, double, bool>> dataForChart3_forward = new List<Tuple<string, string, double, bool>>();
            List<Tuple<string, DateTime, double, bool>> dataForChart5 = new List<Tuple<string, DateTime, double, bool>>(10);
            foreach (var s in selectedContracts)
            {
                if (updateHistoricalData && historicalPrices != null & historicalFundingRates != null)
                {
                    AddManyDataPoints(chart1, s, historicalPrices[i], false, false, false);
                    if (i > 0)
                    {
                        AddManyDataPoints(chart2, s + (ss.futures[s].isPerpetual ? " (RHS)" : ""), historicalFundingRates[i], false, false, ss.futures[s].isPerpetual);
                    }
                    else
                    {
                        AddManyDataPoints(chart2, s, historicalFundingRates[i], false, false, false);
                    }
                }

                dataForChart1.Add(new Tuple<string, DateTime, double, bool>(s, markets.Item1, Math.Round(markets.Item2[i].last), false));

                if (i > 0)
                {
                    double fundingRate = 0;
                    if (ss.futures[s].isPerpetual)
                        fundingRate = ss.fundingRates[s] * 24 * 365.25 * 100;
                    else
                        fundingRate = ss.futures[s].ImpliedFundingRate(markets.Item2[0].last, markets.Item1) * 100;


                    dataForChart2.Add(new Tuple<string, DateTime, double, bool>(s + (ss.futures[s].isPerpetual ? " (RHS)" : ""), markets.Item1, Math.Round(fundingRate, 2), ss.futures[s].isPerpetual));

                    double yf = ss.futures[s].YearFraction(markets.Item1);
                    double forwardRate = (fundingRate * yf - prevRate * prevYF) / (yf - prevYF);

                    dataForChart3_forward.Add(new Tuple<string, string, double, bool>("Forward Rate", s + "(" + Math.Round(yf, 2) + "y)", Math.Round(forwardRate, 2), false));
                    dataForChart3_implied.Add(new Tuple<string, string, double, bool>("Implied Rate", s + "(" + Math.Round(yf, 2) + "y)", Math.Round(fundingRate, 2), false));

                    if (ss.futures[s].isPerpetual) {
                        double previousLevel = (chart5.Series.Count == 2 ? chart5.Series[1].Points.Last().YValues[0] : 1);
                        DateTime previousTimestamp = (chart5.Series.Count == 2 ? DateTime.FromOADate(chart5.Series[1].Points.Last().XValue) : markets.Item1);
                        double dx = (markets.Item1 - previousTimestamp).TotalDays / 365.25;
                        double newLevel = previousLevel * (1 + fundingRate / 100 * dx);

                        dataForChart5.Add(new Tuple<string, DateTime, double, bool>("Funding Rate", markets.Item1, Math.Round(fundingRate, 2), false));
                        dataForChart5.Add(new Tuple<string, DateTime, double, bool>("Accrued Index (RHS)", markets.Item1, newLevel, true)); // don't round this off.
                    }

                    prevYF = yf;
                    prevRate = fundingRate;
                }

                label4.Invoke(new MethodInvoker(delegate
                {
                    label4.Text += s + ":".PadRight(10 - Math.Min(10, s.Length)) + markets.Item2[i].bid + "  /  " + markets.Item2[i].ask + Environment.NewLine;
                }));

                i++;
            }

            AddManyDataPoints(chart1, dataForChart1, false, true);
            AddManyDataPoints(chart2, dataForChart2, false, true);
            AddManyDataPoints(chart3, dataForChart3_forward, true, false);
            AddManyDataPoints(chart3, dataForChart3_implied, false, true);
            AddManyDataPoints(chart5, dataForChart5, false, false);

            if (historicalFundingRates != null)
                updateHistoricalData = false;
        }

        private async void UpdateFundingRates(object state)
        {
            chart4.Invoke(new MethodInvoker(delegate
            {
                foreach (var s in chart4.Series)
                    s.Points.Clear();
            }));

            await ss.UpdateFundingRatesAsync();

            var sorted = (from s in ss.fundingRates
                          orderby s.Value descending
                          select s).Take(10);

            List<Tuple<string, string, double, bool>> data = new List<Tuple<string, string, double, bool>>(10);
            foreach (var s in sorted)
                data.Add(new Tuple<string, string, double, bool>("Token", s.Key, Math.Round(s.Value * 24 * 365.25 * 100, 2), false));

            AddManyDataPoints(chart4, data, true, false);
        }

        // Add many points to a single series in one go
        private static void AddManyDataPoints(Chart chart, string seriesName, IEnumerable<Tuple<DateTime,double>> data, bool isPoint, bool useMarker, bool useY2)
        {
            int n = data.Count();
            List<Tuple<string, DateTime, double, bool>> newData = new List<Tuple<string, DateTime, double, bool>>(n);
            int i = 0;
            foreach (var d in data)
            {
                if (d == null) break;

                newData.Add(new Tuple<string, DateTime, double, bool>(seriesName, d.Item1, d.Item2, useY2));
                i++;
            }

            AddManyDataPoints(chart, newData, isPoint, useMarker);
        }

        // Add many points to different series in one go
        private static void AddManyDataPoints<T>(Chart chart, IEnumerable<Tuple<string, T, double, bool>> data, bool isPoint, bool useMarker)
        {
            chart.Invoke(new MethodInvoker(delegate
            {
                foreach (var d in data)
                {
                    if (chart.Series.FindByName(d.Item1) == null)
                    {
                        Series ss = chart.Series.Add(d.Item1);
                        ss.ChartArea = chart.ChartAreas[0].Name;

                        if (typeof(T) == typeof(DateTime))
                            ss.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Time;

                        //ss.MarkerStyle = MarkerStyle.Circle;
                        ss.ToolTip = d.Item1 + ": (#VALY2, #VALX)";

                        if (isPoint)
                        {
                            ss.ChartType = SeriesChartType.Column;
                        }
                        else
                        {
                            ss.ChartType = SeriesChartType.Line;
                        }

                        if (d.Item4)
                            ss.YAxisType = AxisType.Secondary;
                    }


                    DataPoint pp = new DataPoint();
                    if (isPoint)
                    {
                        pp.Label = "#VALY";
                    }

                    if (useMarker)
                        pp.MarkerStyle = MarkerStyle.Circle;

                    pp.SetValueXY(d.Item2, d.Item3);
                    chart.Series[d.Item1].Points.Add(pp);
                }
            }));
        }

    }
}
