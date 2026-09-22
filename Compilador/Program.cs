using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TranspiladorSimples
{
    class Program
    {
        // Tabela de Símbolos Exaustiva (Mapeia palavras-chave para o Português)
        private static readonly Dictionary<string, string> TabelaSubstituicao = new Dictionary<string, string>
        {
            { "int", "inteiro" }, { "float", "real" }, { "double", "duplo" }, { "char", "caractere" },
            { "string", "texto" }, { "bool", "booleano" }, { "void", "vazio" }, { "var", "variavel" },
            { "if", "se" }, { "else", "senao" }, { "switch", "escolha" }, { "case", "caso" }, { "default", "padrao" },
            { "while", "enquanto" }, { "for", "para" }, { "foreach", "para_cada" }, { "do", "faca" },
            { "break", "interrompa" }, { "continue", "continue" }, { "public", "publico" }, { "private", "privado" },
            { "protected", "protegido" }, { "static", "estatico" }, { "const", "constante" }, { "try", "tente" },
            { "catch", "capture" }, { "finally", "finalmente" }, { "throw", "lance" }, { "true", "verdadeiro" },
            { "false", "falso" }, { "null", "nulo" }, { "return", "retorne" }, { "class", "classe" }
        };

        static void Main(string[] args)
        {
            bool rodando = true;
            while (rodando)
            {
                Console.Clear();
                Console.WriteLine("===================================");
                Console.WriteLine("    COMPILADOR / TRANSPILADOR      ");
                Console.WriteLine("===================================");
                
                // Busca todos os arquivos .txt do diretório atual
                string[] arquivosTxt = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txt");
                List<string> arquivosValidos = new List<string>();

                // Filtra para listar apenas códigos originais (ignora os resultados traduzidos antigos)
                foreach (var arq in arquivosTxt)
                {
                    string nome = Path.GetFileName(arq);
                    if (!nome.StartsWith("resultado_"))
                    {
                        arquivosValidos.Add(arq);
                    }
                }

                Console.WriteLine("Selecione um arquivo de código para processar:\n");
                for (int i = 0; i < arquivosValidos.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {Path.GetFileName(arquivosValidos[i])}");
                }
                
                // Opção fixa definida como 0
                Console.WriteLine("0. Sair do programa");
                
                Console.Write("\nEscolha uma opção: ");
                string entrada = Console.ReadLine() ?? "";

                if (entrada == "0")
                {
                    rodando = false;
                    Console.WriteLine("\nEncerrando o programa. Até logo!");
                }
                else if (int.TryParse(entrada, out int escolha) && escolha > 0 && escolha <= arquivosValidos.Count)
                {
                    string arquivoSelecionado = arquivosValidos[escolha - 1];
                    ProcessarEExecutarCenario(arquivoSelecionado);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[ERRO] Opção inválida! Pressione ENTER para tentar de novo...");
                    Console.ResetColor();
                    Console.ReadLine();
                }
            }
        }

        static void ProcessarEExecutarCenario(string caminhoArquivoOrigem)
        {
            string nomeApenas = Path.GetFileName(caminhoArquivoOrigem);
            string arquivoDestino = "resultado_" + nomeApenas;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($">>> EXECUTANDO CENÁRIO DINÂMICO: {nomeApenas} <<<\n");
            Console.ResetColor();

            // 1. Faz a tradução precisa caractere por caractere
            string codigoTraduzido = ProcessarArquivo(caminhoArquivoOrigem);
            File.WriteAllText(arquivoDestino, codigoTraduzido, Encoding.UTF8);

            Console.WriteLine("--- CÓDIGO FONTE TRADUZIDO (.txt) ---");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(codigoTraduzido);
            Console.ResetColor();
            Console.WriteLine("-------------------------------------\n");

            // 2. Executa o interpretador inteligente adaptativo
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- SAÍDA DA EXECUÇÃO TRADUZIDA ---");
            Console.ResetColor();
            
            InterpretarCodigoTraduzido(arquivoDestino);
            
            Console.WriteLine("------------------------------------");
            Console.WriteLine("\nPressione [ENTER] para voltar ao banco de exemplos...");
            Console.ReadLine();
        }

        private static string ProcessarArquivo(string caminhoArquivo)
        {
            string conteudoOriginal = File.ReadAllText(caminhoArquivo);
            StringBuilder resultado = new StringBuilder();
            StringBuilder palavraAtual = new StringBuilder();

            foreach (char c in conteudoOriginal)
            {
                // Se for letra, número ou underline, faz parte de um identificador ou palavra reservada
                if (char.IsLetterOrDigit(c) || c == '_') {
                    palavraAtual.Append(c);
                }
                else {
                    // Chegou a um delimitador (espaço, operador, quebra de linha...)
                    if (palavraAtual.Length > 0) {
                        string palavra = palavraAtual.ToString();
                        
                        // Substitui se estiver na Tabela de Símbolos, senão mantém a palavra original
                        resultado.Append(TabelaSubstituicao.ContainsKey(palavra) ? TabelaSubstituicao[palavra] : palavra);
                        palavraAtual.Clear();
                    }
                    resultado.Append(c); // Mantém o caractere original (espaço, parênteses, chaves, etc)
                }
            }

            // Tratamento preventivo se o arquivo terminar sem pontuação
            if (palavraAtual.Length > 0) {
                string palavra = palavraAtual.ToString();
                resultado.Append(TabelaSubstituicao.ContainsKey(palavra) ? TabelaSubstituicao[palavra] : palavra);
            }
            return resultado.ToString();
        }

        private static void InterpretarCodigoTraduzido(string caminhoArquivo)
        {
            string[] linhas = File.ReadAllLines(caminhoArquivo);
            
            // Tabelas dinâmicas de símbolos em runtime para suportar mudanças de código
            Dictionary<string, string> memoriaVariaveis = new Dictionary<string, string>();
            Dictionary<string, string> tiposVariaveis = new Dictionary<string, string>();

            foreach (string linha in linhas)
            {
                string linhaLimpa = linha.Trim();
                if (string.IsNullOrEmpty(linhaLimpa)) continue;

                // Captura declarações de variáveis dinamicamente independente de espaços extras
                // Padrão suportado: (tipo) (nome_da_variavel) = (valor);
                var matchDeclaracao = Regex.Match(linhaLimpa, @"^(inteiro|real|texto|booleano|variavel)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*([^;]+);");
                
                if (matchDeclaracao.Success)
                {
                    string tipo = matchDeclaracao.Groups[1].Value;
                    string nomeVar = matchDeclaracao.Groups[2].Value.Trim();
                    string valorRaw = matchDeclaracao.Groups[3].Value.Trim();

                    tiposVariaveis[nomeVar] = tipo;
                    memoriaVariaveis[nomeVar] = valorRaw.Trim('"'); // Remove aspas se for do tipo texto

                    Console.WriteLine($"[Alocação Dinâmica]: Variável '{nomeVar}' ({tipo}) setada para -> {valorRaw}");
                    continue;
                }

                // Monitoramento descritivo de controle de fluxo e laços para a apresentação
                if (linhaLimpa.StartsWith("se ") || linhaLimpa.StartsWith("se(")) {
                    Console.WriteLine($"[Fluxo Condicional]: Avaliando decisão estruturada: {linhaLimpa}");
                }
                else if (linhaLimpa.StartsWith("enquanto ") || linhaLimpa.StartsWith("enquanto(")) {
                    Console.WriteLine($"[Fluxo Iterativo]: Monitorando laço de repetição: {linhaLimpa}");
                }
                
                // Captura a instrução de saída de forma flexível: retorne (variavel ou valor direto);
                var matchRetorno = Regex.Match(linhaLimpa, @"^retorne\s+([^;]+);");
                if (matchRetorno.Success)
                {
                    string alvo = matchRetorno.Groups[1].Value.Trim();

                    if (memoriaVariaveis.ContainsKey(alvo))
                    {
                        string tipo = tiposVariaveis[alvo];
                        string valor = memoriaVariaveis[alvo];
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n>> RETORNO ADAPTATIVO ({tipo}): {valor}");
                        Console.ResetColor();
                    }
                    else
                    {
                        // Se retornou um valor literal direto (Ex: retorne 200;)
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n>> RETORNO ADAPTATIVO (literal): {alvo}");
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
