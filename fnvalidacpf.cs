using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace httpValidaCpf
{
    public class FnValidaCpf
    {
        private readonly ILogger<FnValidaCpf> _logger;

        public FnValidaCpf(ILogger<FnValidaCpf> logger)
        {
            _logger = logger;
        }

        class Result
        {
            public string Message { get; set; }
            public bool Valid { get; set; }
        }

        [Function("FnValidaCpf")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Iniciando validação do CPF.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = JsonConvert.DeserializeObject(requestBody);

            if (data == null)
                return new BadRequestObjectResult("Informe um CPF a ser validado.");

            string cpf = data!.cpf;

            if (string.IsNullOrEmpty(cpf) || cpf.Length < 11)
                return new BadRequestObjectResult("Informe um CPF em formato valido.");

            bool cpfValido = CpfValido(cpf);

            return new OkObjectResult(new Result()
            {
                Message = cpfValido ? "O CPF inserido é válido." : "O CPF inserido é inválido.",
                Valid = cpfValido
            });
        }

        private bool CpfValido(string cpf)
        {
            int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

            cpf = cpf.Trim().Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;

            for (int j = 0; j < 10; j++)
                if (j.ToString().PadLeft(11, char.Parse(j.ToString())) == cpf)
                    return false;

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf += digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }
    }
}
