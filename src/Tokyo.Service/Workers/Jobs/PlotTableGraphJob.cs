using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Workers.Jobs
{
    public class PlotTableGraphJob
    {
        // TODO make sure resulting picture format is free (license)

        // TODO current plan: prepare data from time tables, send as .json to 2nd python server and consume produced results from blob-store (currently mssql binary column)
        /*using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));//"application/json"));
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:6666/secret/qqq");
                //string requestBody = JsonConvert.SerializeObject(new { X_AXIS_TICKS = DateTime.UtcNow });

                var startAndEndDate = selectedTable.GetStartAndEndDate();
                var tableTimeRangeStartDate = startAndEndDate.Item1.Date;
                var tableTimeRangeEndDate = startAndEndDate.Item2.Date;
                var tableTimeRangeDays = new List<string>();
                const int MAX_DAYS = 200;
                while (tableTimeRangeStartDate.Day <= tableTimeRangeEndDate.Day)
                {
                    tableTimeRangeDays.Add(tableTimeRangeStartDate.ToShortDateString());
                    tableTimeRangeStartDate = tableTimeRangeStartDate.AddDays(1.0);
                    if (tableTimeRangeDays.Count > MAX_DAYS)
                    {
                        break;
                    }
                }

                string requestBody = JsonConvert.SerializeObject(new TimeTablePlotData()
                {
                    X_AXIS_TICKS = tableTimeRangeDays.ToArray(),
                    TIME_TABLE_ID = selectedTable.TimeTableId
                });
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");//"application/x-www-form-urlencoded");
                //Task<HttpResponseMessage> httpRequest = httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                var isSuccess = response.IsSuccessStatusCode;
                if (isSuccess)
                {
                    var responseContent = await response.Content.ReadAsByteArrayAsync();
                    Console.WriteLine(responseContent);
                }
            }*/
    }
}
