using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace cpfAPI
{
    public static class cpfCheckAPI
    {
        [FunctionName("cpfCheckAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Recebendo dados para serem validados...");

            string requestBody;
            dynamic data;
            try
            {
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                data = JsonConvert.DeserializeObject(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError($"Erro ao ler o corpo da requisição: {ex.Message}");
                return new BadRequestObjectResult("Erro ao processar a requisição");
            }

            if (data == null || data.cpf == null)
            {
                return new BadRequestObjectResult("Informe um CPF válido");
            }

            string cpf = data.cpf.ToString();

            if (string.IsNullOrEmpty(cpf))
            {
                return new BadRequestObjectResult("Informe um CPF válido");
            }

            if (ValidaCPF(cpf) == false)
            {
                return new BadRequestObjectResult("CPF inválido");
            }

            return new OkObjectResult("CPF válido");
        }

        private static bool ValidaCPF(string cpf)
        {
            if (cpf.Length != 11)
            {
                return false;
            }

            int[] v = new int[11];
            for (int i = 0; i < 11; i++)
            {
                v[i] = int.Parse(cpf[i].ToString());
            }

            int soma = 0;
            for (int i = 0, j = 10; i < 9; i++, j--)
            {
                soma += v[i] * j;
            }

            int dig1 = soma % 11;
            if (dig1 < 2)
            {
                dig1 = 0;
            }
            else
            {
                dig1 = 11 - dig1;
            }

            soma = 0;
            for (int i = 0, j = 11; i < 10; i++, j--)
            {
                soma += v[i] * j;
            }

            int dig2 = soma % 11;
            if (dig2 < 2)
            {
                dig2 = 0;
            }
            else
            {
                dig2 = 11 - dig2;
            }

            if (dig1 == v[9] && dig2 == v[10])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}