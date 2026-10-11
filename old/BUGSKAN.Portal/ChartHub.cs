using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using static BUGSKAN.Portal.ChartDataUpdate;
using Microsoft.AspNet.SignalR.Infrastructure;


namespace BUGSKAN.Portal
{
    public class ChartHub : Hub
    {
        private readonly ChartDataUpdate _ChartInstance;
        public ChartHub() : this(ChartDataUpdate.Instance) { }

        public ChartHub(ChartDataUpdate ChartInstance)
        {
            _ChartInstance = ChartInstance;
        }

        public void InitChartData()
        {
            //Show Chart initially when InitChartData called first time    
            LineChart lineChart = new LineChart();
            PieChart pieChart = new PieChart();
            lineChart.SetLineChartData();
            pieChart.SetPieChartData();
            GetAllClients().All.UpdateChart(lineChart, pieChart);


            //Call GetChartData to send Chart data every 5 seconds    
            _ChartInstance.GetChartData();

        }
        private static dynamic GetAllClients()
        {
            return GlobalHost.ConnectionManager.GetHubContext<ChartHub>().Clients;
        }
    }
}