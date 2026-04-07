using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Green_Analizer.Models;

namespace Green_Analizer.Service
{
    public class ApiService
    {
        private static readonly HttpClient _client = new HttpClient();

        public async Task<List<DatoAmbientale>> ScaricaDatiAsync(string lat, string lon)
        {
            List<DatoAmbientale> listaDati = new List<DatoAmbientale>();

            try
            {
                string dataInizio = "2013-01-03";
                string dataFine = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd");

                string urlMeteo = $"https://archive-api.open-meteo.com/v1/archive?latitude={lat}&longitude={lon}&start_date={dataInizio}&end_date={dataFine}&hourly=temperature_2m,precipitation,wind_speed_10m&timezone=Europe%2FRome";
                string urlInquinamento = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={lat}&longitude={lon}&start_date={dataInizio}&end_date={dataFine}&hourly=pm10,pm2_5,nitrogen_dioxide,european_aqi_pm10,european_aqi_pm2_5,european_aqi_nitrogen_dioxide&timezone=Europe%2FRome";

                string jsonMeteo = await _client.GetStringAsync(urlMeteo);
                string jsonInquinamento = await _client.GetStringAsync(urlInquinamento);

                JsonNode nodoMeteo = JsonNode.Parse(jsonMeteo);
                JsonNode nodoInquinamento = JsonNode.Parse(jsonInquinamento);

                // Array Meteo
                var dateMeteo = nodoMeteo["hourly"]["time"].AsArray();
                var tempArray = nodoMeteo["hourly"]["temperature_2m"].AsArray();
                var precArray = nodoMeteo["hourly"]["precipitation"].AsArray();
                var ventoArray = nodoMeteo["hourly"]["wind_speed_10m"].AsArray();

                // Array Inquinamento
                var pm10Array = nodoInquinamento["hourly"]["pm10"].AsArray();
                var pm25Array = nodoInquinamento["hourly"]["pm2_5"].AsArray();
                var no2Array = nodoInquinamento["hourly"]["nitrogen_dioxide"].AsArray();
                var aqiPm10Array = nodoInquinamento["hourly"]["european_aqi_pm10"].AsArray();
                var aqiPm25Array = nodoInquinamento["hourly"]["european_aqi_pm2_5"].AsArray();
                var aqiNo2Array = nodoInquinamento["hourly"]["european_aqi_nitrogen_dioxide"].AsArray();

                int oreTotali = dateMeteo.Count;
                int giorniTotali = oreTotali / 24;

                // Calcolo delle medie giornaliere
                for (int d = 0; d < giorniTotali; d++)
                {
                    DatoAmbientale dato = new DatoAmbientale();
                    dato.Data = DateTime.Parse(dateMeteo[d * 24].ToString()).Date;

                    double sumTemp = 0, sumPrec = 0, sumVento = 0;
                    double sumPm10 = 0, sumPm25 = 0, sumNo2 = 0;
                    double sumAqiPm10 = 0, sumAqiPm25 = 0, sumAqiNo2 = 0;

                    int validMeteo = 0, validInquinamento = 0;

                    for (int h = 0; h < 24; h++)
                    {
                        int i = (d * 24) + h;

                        // Meteo
                        if (tempArray[i] != null)
                        {
                            sumTemp += tempArray[i].GetValue<double>();
                            sumPrec += precArray[i]?.GetValue<double>() ?? 0;
                            sumVento += ventoArray[i]?.GetValue<double>() ?? 0;
                            validMeteo++;
                        }

                        // Inquinamento
                        if (pm10Array.Count > i && pm10Array[i] != null)
                        {
                            sumPm10 += pm10Array[i].GetValue<double>();
                            sumPm25 += pm25Array[i]?.GetValue<double>() ?? 0;
                            sumNo2 += no2Array[i]?.GetValue<double>() ?? 0;

                            sumAqiPm10 += aqiPm10Array[i]?.GetValue<double>() ?? 0;
                            sumAqiPm25 += aqiPm25Array[i]?.GetValue<double>() ?? 0;
                            sumAqiNo2 += aqiNo2Array[i]?.GetValue<double>() ?? 0;

                            validInquinamento++;
                        }
                    }

                    if (validMeteo > 0 && validInquinamento > 0)
                    {
                        dato.TemperaturaMedia = Math.Round(sumTemp / validMeteo, 2);
                        dato.PrecipitazioniTotali = Math.Round(sumPrec, 2);
                        dato.VentoMedia = Math.Round(sumVento / validMeteo, 2);

                        dato.PM10 = Math.Round(sumPm10 / validInquinamento, 2);
                        dato.PM25 = Math.Round(sumPm25 / validInquinamento, 2);
                        dato.NO2 = Math.Round(sumNo2 / validInquinamento, 2);

                        dato.AqiPm10 = Math.Round(sumAqiPm10 / validInquinamento, 2);
                        dato.AqiPm25 = Math.Round(sumAqiPm25 / validInquinamento, 2);
                        dato.AqiNo2 = Math.Round(sumAqiNo2 / validInquinamento, 2);

                        listaDati.Add(dato);
                    }
                }

                return listaDati;
            }
            catch (Exception ex)
            {
                throw new Exception($"Errore API per coordinate {lat}, {lon}: {ex.Message}");
            }
        }
    }
}