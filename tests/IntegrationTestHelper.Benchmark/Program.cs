using IntegrationTestHelper;
using IntegrationTestHelper.Caches;
using IntegrationTestHelper.Request;
using IntegrationTestHelper.WebAPI;
using IntegrationTestHelper.WebAPI.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new WebHostBuilder()
                            .UseEnvironment("Development")
                            .UseStartup<Startup>()
                            .ConfigureTestServices(services =>
                                services
                                    .LoadEndpoints());

            using var server = new TestServer(builder);
            var client = server.CreateClient();

            var act = Request.FromAction<WeatherForecastController>(c => c.Post(new WeatherForecast { Date = DateTime.Now, Summary = "Teste", TemperatureC = -20 }, "route-with-custom-name", "route-without-custom-name", "header-with-custom-name", "header-without-custom-name", "query-with-custom-name", "query-without-custom-name", new string[] { "item i", "item-2" }));
            var request = act.Build();

            var response = await client.SendAsync(request);

            var str = await response.Content.ReadAsStringAsync();

        }
    }

    class CpfNumberGenerator
    {
        public static string Generate()
        {
            var random = new Random();
            var cpf = new byte[11];
            int firstDigitSommatory = 0;
            int secondDigitSommatory = 0;
            for (int index = 0, firstDigitWeight = 10, secondDigitWeight = 11; index < 9; index++, firstDigitWeight--, secondDigitWeight--)
            {
                var digit = Convert.ToByte(random.Next(0, 10));
                cpf[index] = digit;
                firstDigitSommatory += digit * firstDigitWeight;
                secondDigitSommatory += digit * secondDigitWeight;
            }
            var digitQuotient = firstDigitSommatory % 11;
            cpf[9] = Convert.ToByte(digitQuotient < 2 ? 0 : 11 - digitQuotient);
            secondDigitSommatory += cpf[9] * 2;

            digitQuotient = secondDigitSommatory % 11;
            cpf[10] = Convert.ToByte(digitQuotient < 2 ? 0 : 11 - digitQuotient);

            return string.Join("", cpf);
        }
    }

    class ValidaCPF
    {
        public static bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;
            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }
    }
}
