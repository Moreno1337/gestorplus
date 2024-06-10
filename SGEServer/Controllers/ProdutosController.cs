using SGEServer.Class;
using SGEServer.Data;
using SGEServer.Models;
using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.InkML;
using System.Reflection.Metadata;


namespace SGEServer.Controllers
{
    public class ProdutosController
    {
        public static async Task TesteNcm(string ncmCode)
        {
            string url = $"https://brasilapi.com.br/api/ncm/v1/{ncmCode}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
        }

        public string SalvarProduto(ProdutosModel dados)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> list = new()
                {

                    new ParametroSql("input", "200", "Nome", dados.Item!),
                    new ParametroSql("input", "200", "Marca", dados.Marca!),
                    new ParametroSql("input", "200", "Ncm", dados.Ncm!),
                    new ParametroSql("input", "200", "Ca", dados.CaEpi!),
                    new ParametroSql("input", "50", "Categoria", dados.CodCategoria!),
                    new ParametroSql("input", "50", "Fornecedor", dados.Fornecedor!),
                    new ParametroSql("input", "", "EstoqueMinimo", dados.EstoqueMinimo),
                    new ParametroSql("input", "10", "EnderecoEstoque", dados.Endereco!),
                    new ParametroSql("input", "500", "Descricao", dados.Descricao!),
                    new ParametroSql("input", "", "UnidadeMedida", dados.CodUnidadeMedida!),
                    new ParametroSql("input", "", "CodEmpresa", UserInfo.CodEmpresa),
                    new ParametroSql("output", "500", "Resp", 0),
                };

                return contexto.ExecutaComandoRetornoString("[SpOperacional001CadastroProdutos]", list);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public List<DropDownModel> CarregaCategorias()
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<DropDownModel> dados = new List<DropDownModel>();

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "", "CodEmpresa", UserInfo.CodEmpresa),
                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("SpOperacional002CarregaCategorias", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        dados.Add(new()
                        {
                            Codigo = int.Parse(It["Codigo"].ToString()!),
                            Descricao = It["Descricao"].ToString()!,
                        });
                    }
                }
                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public List<ProdutosModel> CarregaProdutosCadastrados(int CodEmpresa, string nome, string Ncm, string Ca, string Marca)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<ProdutosModel> dados = new List<ProdutosModel>();

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "200", "Nome", nome),
                    new ParametroSql("input", "200", "Ncm", Ncm),
                    new ParametroSql("input", "200", "Ca", Ca),
                    new ParametroSql("input", "200", "Marca", Marca),
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa)
                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("SpConsulta001RetornaProdutosCadastrados", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        var produto = new ProdutosModel();

                        if (int.TryParse(It["Codigo"].ToString(), out int codProduto))
                        {
                            produto.CodProduto = codProduto;
                        }

                        produto.Item = It["Nome"].ToString();
                        produto.Marca = It["Marca"].ToString();
                        produto.Referencia = It["Referencia"].ToString();
                        produto.Ncm = It["Ncm"].ToString();
                        produto.CaEpi = It["Ca"].ToString();
                        produto.CodBarras = It["CodBarras"].ToString();
                        produto.UnidadeMedida = It["UnidadeMedida"].ToString();

                        if (int.TryParse(It["LucroVenda"].ToString(), out int lucroVenda))
                        {
                            produto.LucroVenda = lucroVenda;
                        }

                        if (decimal.TryParse(It["ValorVendaIndicado"].ToString(), out decimal valorVendaIndicado))
                        {
                            produto.ValorVendaIndicado = valorVendaIndicado;
                        }
                        if (int.TryParse(It["QuantidadeEstoque"].ToString(), out int quantidadeEstoque))
                        {
                            produto.QuantidadeEstoque = quantidadeEstoque;
                        }

                        produto.EnderecoEstoque = It["EnderecoEstoque"].ToString();

                        if (int.TryParse(It["EstoqueMinimo"].ToString(), out int estoqueMinimo))
                        {
                            produto.EstoqueMinimo = estoqueMinimo;
                        }

                        if (It["ValorUltimaCompra"] != DBNull.Value && decimal.TryParse(It["ValorUltimaCompra"].ToString(), out decimal valorUltimaCompra))
                        {
                            produto.ValorUltimaCompra = valorUltimaCompra;
                        }

                        if (It["ValorUltimaVenda"] != DBNull.Value && decimal.TryParse(It["ValorUltimaVenda"].ToString(), out decimal valorUltimaVenda))
                        {
                            produto.ValorUltimaVenda = valorUltimaVenda;
                        }

                        dados.Add(produto);
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }


        public string RemoverProduto(int codProduto)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> list = new()
                {

                    new ParametroSql("input", "", "CodProduto", codProduto),
                    new ParametroSql("input", "", "CodEmpresa", UserInfo.CodEmpresa),
                    new ParametroSql("output", "500", "Resp", 0),
                };

                return contexto.ExecutaComandoRetornoString("[SpOperacional003RemoverProdutoCadastrado]", list);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public string RemoverProdutoPrazo(int CodProdutoAPrazo, int CodEmpresa)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> list = new()
                {

                    new ParametroSql("input", "", "CodProdutoPendente", CodProdutoAPrazo),
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa),
                    new ParametroSql("output", "500", "Resp", 0),
                };

                return contexto.ExecutaComandoRetornoString("SpOperacional042RemoveProdutoAPrazo", list);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public ProdutosModel CarregaProdutoPorCod(int codProduto)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                ProdutosModel dados = new ProdutosModel();

                List<ParametroSql> list = new()
        {
            new ParametroSql("input", "", "CodigoProduto", codProduto),
            new ParametroSql("input", "", "CodEmpresa", UserInfo.CodEmpresa),
        };

                DataTable Dt = contexto.ExecutaComandoRetornoData("SpOperacional004RetornaProdutoPorCod", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        if (int.TryParse(It["Codigo"].ToString(), out int codProdutoParsed))
                        {
                            dados.CodProduto = codProdutoParsed;
                        }

                        dados.Item = It["Nome"].ToString();
                        dados.Referencia = It["Referencia"].ToString();
                        dados.Marca = It["Marca"].ToString();
                        dados.Ncm = It["Ncm"].ToString();
                        dados.CaEpi = It["Ca"].ToString();

                        if (int.TryParse(It["CodCategoria"].ToString(), out int codCategoriaParsed))
                        {
                            dados.CodCategoria = codCategoriaParsed;
                        }

                        dados.Fornecedor = It["Fornecedor"].ToString();

                        if (int.TryParse(It["EstoqueMinimo"].ToString(), out int estoqueMinimoParsed))
                        {
                            dados.EstoqueMinimo = estoqueMinimoParsed;
                        }

                        if (int.TryParse(It["CodUnidadeMedida"].ToString(), out int codUnidadeMedidaParsed))
                        {
                            dados.CodUnidadeMedida = codUnidadeMedidaParsed;
                        }

                        dados.Endereco = It["EnderecoEstoque"].ToString();
                        dados.Descricao = It["Descricao"].ToString();

                        if (int.TryParse(It["QtdeEstoque"].ToString(), out int quantidadeEstoqueParsed))
                        {
                            dados.QuantidadeEstoque = quantidadeEstoqueParsed;
                        }
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }


        public string EditarProduto(ProdutosModel dados)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> list = new()
                {

                    new ParametroSql("input", "", "CodProduto", dados.CodProduto),
                    new ParametroSql("input", "200", "Nome", dados.Item!),
                    new ParametroSql("input", "200", "Marca", dados.Marca!),
                    new ParametroSql("input", "200", "Ncm", dados.Ncm!),
                    new ParametroSql("input", "200", "Ca", dados.CaEpi!),
                    new ParametroSql("input", "50", "Categoria", dados.CodCategoria!),
                    new ParametroSql("input", "50", "Fornecedor", dados.Fornecedor!),
                    new ParametroSql("input", "", "EstoqueMinimo", dados.EstoqueMinimo),
                    new ParametroSql("input", "10", "EnderecoEstoque", dados.Endereco!),
                    new ParametroSql("input", "500", "Descricao", dados.Descricao!),
                    new ParametroSql("input", "", "CodUnidadeMedida", dados.CodUnidadeMedida!),
                    new ParametroSql("input", "", "CodEmpresa", UserInfo.CodEmpresa),
                    new ParametroSql("output", "500", "Resp", 0),
                };

                return contexto.ExecutaComandoRetornoString("[SpOperacional005EditarProduto]", list);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public List<DropDownModel> CarregaUnidadesMedida()
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<DropDownModel> dados = new List<DropDownModel>();

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "", "CodEmpresa", UserInfo.CodEmpresa),
                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("SpOperacional006CarregaUnidadesMedida", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        dados.Add(new()
                        {
                            Codigo = int.Parse(It["Codigo"].ToString()!),
                            Descricao = It["Descricao"].ToString()!,
                            DescricaoUnidadeMedida = It["Descricao"].ToString()!,
                        });
                    }
                }
                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public string RealizarEntradaEstoque(int codProduto, string Item, decimal valorUltimaCompra, int Quantidade, int LucroVenda, decimal ValorTotal, decimal Icms, decimal Ipi, int codEmpresa)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> parametros = new()
            {
                new ParametroSql("input", "", "CodProduto", codProduto),
                new ParametroSql("input", "", "Nome", Item),
                new ParametroSql("input", "18,6", "ValorUltimaCompra", valorUltimaCompra),
                new ParametroSql("input", "", "Quantidade", Quantidade),
                new ParametroSql("input", "", "LucroVenda", LucroVenda),
                new ParametroSql("input", "18,6", "ValorVendaIndicado", ValorTotal),
                new ParametroSql("input", "18,6", "Icms", Icms),
                new ParametroSql("input", "18,6", "Ipi", Ipi),
                new ParametroSql("input", "", "CodEmpresa", codEmpresa),
                new ParametroSql("output", "500", "Resp", 0),
            };

                return contexto.ExecutaComandoRetornoString("[SpOperacional008EntradaEstoque]", parametros);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }
        public string RealizaSaidaEstoque(int CodEmpresa, ProdutosModel dados)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> parametros = new()
            {
                new ParametroSql("input", "", "CodProduto", dados.CodProduto),
                new ParametroSql("input", "", "ValorUltimaVenda", dados.ValorUnitario),
                new ParametroSql("input", "", "ValorTotal", dados.ValorTotal),
                new ParametroSql("input", "200", "Nome", dados.Item),
                new ParametroSql("input", "", "Quantidade", dados.QtdeItens),
                new ParametroSql("input", "18", "Cnpj", dados.Cnpj),
                new ParametroSql("input", "200", "NomeFantasia", dados.NomeFantasia),
                new ParametroSql("input", "18", "Cpf", dados.Cpf),
                new ParametroSql("input", "200", "NomePessoa", dados.NomePessoa),
                new ParametroSql("input", "", "CodCliente", dados.CodCliente),
                new ParametroSql("input", "50", "RetiradoPor", dados.RetiradoPor),
                new ParametroSql("input", "10", "TipoMovimentacao", dados.TipoMovimentacao),
                new ParametroSql("input", "", "CodEmpresa", CodEmpresa),
                new ParametroSql("output", "500", "Resp", 0),
            };

                return contexto.ExecutaComandoRetornoString("SpOperacional009SaidaEstoque", parametros);

            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }


        public List<MovimentacaoModel> CarregaMovimentacaoEntrada(int CodEmpresa, string nome)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<MovimentacaoModel> dados = new List<MovimentacaoModel>();

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "200", "Nome", nome),
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa)
                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("[SpConsulta003RetornaMovimentacaoEntrada]", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        dados.Add(new()
                        {
                            CodProduto = int.Parse(It["CodProduto"].ToString()!),
                            IdMovimentacao = int.Parse(It["IdMovimentacao"].ToString()!),
                            Item = It["Nome"].ToString()!,
                            TipoMovimentacao = It["TipoMovimentacao"].ToString()!,
                            Quantidade = int.Parse(It["Quantidade"].ToString()!),
                            Icms = decimal.Parse(It["Icms"].ToString()!),
                            Ipi = decimal.Parse(It["Ipi"].ToString()!),
                            LucroVenda = int.Parse(It["LucroVenda"].ToString()!),
                            ValorUnitario = decimal.Parse(It["ValorMovimentacao"].ToString()!),
                            ValorTotal = decimal.Parse(It["ValorTotal"].ToString()!),
                            DataMovimentacao = DateTime.Parse(It["DataMovimentacao"].ToString()),
                        });
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public List<MovimentacaoModel> CarregaMovimentacaoSaida(int CodEmpresa, string nome)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<MovimentacaoModel> dados = new List<MovimentacaoModel>();

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "200", "Nome", nome),
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa)
                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("[SpConsulta004RetornaMovimentacaoSaida]", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        var movimentacao = new MovimentacaoModel
                        {
                            Item = It["Nome"].ToString(),
                            TipoMovimentacao = It["TipoMovimentacao"].ToString(),
                            DataMovimentacao = DateTime.Parse(It["DataMovimentacao"].ToString()),
                            Cnpj = It["Cnpj"].ToString(),
                            NomeFantasia = It["NomeFantasia"].ToString(),
                            Cpf = It["Cpf"].ToString(),
                            NomePessoa = It["NomePessoa"].ToString(),
                            RetiradoPor = It["RetiradoPor"].ToString(),
                            RefCotacao = It["RefCotacao"].ToString(),
                            RefOrdemCompra = It["RefOrdemCompra"].ToString(),
                        };

                        if (int.TryParse(It["CodProduto"].ToString(), out int codProduto))
                            movimentacao.CodProduto = codProduto;

                        if (int.TryParse(It["IdMovimentacao"].ToString(), out int idMovimentacao))
                            movimentacao.IdMovimentacao = idMovimentacao;

                        if (int.TryParse(It["Quantidade"].ToString(), out int quantidade))
                            movimentacao.Quantidade = quantidade;

                        if (decimal.TryParse(It["ValorMovimentacao"].ToString(), out decimal valorUnitario))
                            movimentacao.ValorUnitario = valorUnitario;

                        if (decimal.TryParse(It["ValorTotal"].ToString(), out decimal valorTotal))
                            movimentacao.ValorTotal = valorTotal;

                        dados.Add(movimentacao);
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {

                Console.WriteLine(exp.ToString());
                throw;
            }
        }


        public List<ProdutosModel> CarregarProdutosAPrazoPorEmpresa(int codEmpresa, int CodCliente)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<ProdutosModel> dados = new List<ProdutosModel>();

                List<ParametroSql> parametros = new List<ParametroSql>
                {
                    new ParametroSql("input", "", "CodEmpresa", codEmpresa),
                    new ParametroSql("input", "", "CodCliente", CodCliente),
                };

                DataTable dt = contexto.ExecutaComandoRetornoData("[SpConsulta008ProdutosAPrazoPorEmpresa]", parametros);

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        dados.Add(new ProdutosModel
                        {
                            CodProdutoAPrazo = int.Parse(row["Codigo"].ToString()),
                            CodCliente = int.Parse(row["CodCliente"].ToString()),
                            CodProduto = int.Parse(row["CodProduto"].ToString()),
                            Item = row["Item"].ToString(),
                            QtdeItens = int.Parse(row["Quantidade"].ToString()),
                            ValorUnitario = decimal.Parse(row["ValorUnitario"].ToString()),
                            ValorTotal = decimal.Parse(row["ValorTotal"].ToString()),
                            DataVenda = DateTime.Parse(row["DataVenda"].ToString()),
                            DataVencimento = DateTime.Parse(row["DataVencimento"].ToString()),
                            StatusPagamento = row["StatusPagamento"].ToString(),
                            NomeFantasia = row["NomeFantasia"].ToString(),
                            NomePessoa = row["NomePessoa"].ToString(),
                            Cnpj = row["Cnpj"].ToString(),
                            Cpf = row["Cpf"].ToString(),
                        });
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {

                throw new Exception($"Erro ao carregar produtos a prazo: {exp.Message}");

            }
        }

        public List<ProdutosModel> CarregarHistoricoProdutosAPrazoPorEmpresa(int codEmpresa, int CodCliente)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<ProdutosModel> dados = new List<ProdutosModel>();

                List<ParametroSql> parametros = new List<ParametroSql>
                {
                    new ParametroSql("input", "", "CodEmpresa", codEmpresa),
                    new ParametroSql("input", "", "CodCliente", CodCliente),
                };

                DataTable dt = contexto.ExecutaComandoRetornoData("[SpConsulta019HistoricoProdutosAPrazoPorEmpresa]", parametros);

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {

                        dados.Add(new ProdutosModel
                        {
                            CodProdutoAPrazo = int.Parse(row["Codigo"].ToString()),
                            CodCliente = int.Parse(row["CodCliente"].ToString()),
                            CodProduto = int.Parse(row["CodProduto"].ToString()),
                            Item = row["Item"].ToString(),
                            QtdeItens = int.Parse(row["Quantidade"].ToString()),
                            ValorUnitario = decimal.Parse(row["ValorUnitario"].ToString()),
                            ValorTotal = decimal.Parse(row["ValorTotal"].ToString()),
                            DataVenda = DateTime.Parse(row["DataVenda"].ToString()),
                            DataVencimento = DateTime.Parse(row["DataVencimento"].ToString()),
                            StatusPagamento = row["StatusPagamento"].ToString(),
                        });
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {

                throw new Exception($"Erro ao carregar produtos a prazo: {exp.Message}");

            }
        }

        public string InserirValorAbater(decimal ValorAbater, int CodCliente, int CodEmpresa, string NomeResponsavel)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "", "ValorAbater", ValorAbater),
                    new ParametroSql("input", "", "NomeResponsavel", NomeResponsavel),
                    new ParametroSql("input", "", "CodCliente", CodCliente),
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa),
                    new ParametroSql("output", "500", "Resp", 0),
                };

                return contexto.ExecutaComandoRetornoString("[SpOperacional040AdicionarCreditosAEmpresa]", list);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }
        public string InserirValorRetirar(decimal ValorRetirar,string MotivoRetirada , int CodCliente, int CodEmpresa, string NomeResponsavel)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "", "ValorRetirar", ValorRetirar),
                    new ParametroSql("input", "", "MotivoRetirada", MotivoRetirada),
                    new ParametroSql("input", "", "NomeResponsavel", NomeResponsavel),
                    new ParametroSql("input", "", "CodCliente", CodCliente),
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa),
                    new ParametroSql("output", "500", "Resp", 0),
                    //LEMBRA DE NO BANCO ADICIONAR O DIA EM QUE FOI RETIRADO
                };

                return contexto.ExecutaComandoRetornoString("", list);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public List<ProdutosModel> CarregaTransacoesEmpresa(int CodEmpresa, string TipoTransacaoFiltro, string NomeFiltro, decimal ValorFiltro, DateTime? DataTransacaoFiltro)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<ProdutosModel> dados = new List<ProdutosModel>();

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa),
                    new ParametroSql("input", "", "TipoTransacaoFiltro", TipoTransacaoFiltro),
                    new ParametroSql("input", "", "NomeFiltro", NomeFiltro),
                    new ParametroSql("input", "", "ValorFiltro", ValorFiltro),
                    new ParametroSql("input", "", "DataTransacaoFiltro", DataTransacaoFiltro)
                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        var movimentacoes = new ProdutosModel();

                        movimentacoes.TipoTransacao = It["TipoTransacao"].ToString();
                        movimentacoes.NomeResponsavel = It["NomeResponsavel"].ToString();
                        movimentacoes.ValorTransacao = decimal.Parse(It["ValorTransacao"].ToString());
                        movimentacoes.DataTransacao = DateTime.Parse(It["DataTransacao"].ToString());
                        movimentacoes.MotivoRetirada = It["MotivoTransacao"].ToString();
                        dados.Add(movimentacoes);
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public List<FormModel> CarregaPlanos()
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<FormModel> dados = new List<FormModel>();

                List<ParametroSql> list = new()
                {
                   
                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("SpOperacional044CarregaPlanos", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        dados.Add(new()
                        {
                            Codigo = int.Parse(It["Codigo"].ToString()!),
                            Descricao = It["Descricao"].ToString()!,
                        });
                    }
                }
                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public string SalvarForm(FormModel dados)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> list = new()
                {

                    new ParametroSql("input", "200", "Email", dados.Email!),
                    new ParametroSql("input", "200", "DuvidaFeedback", dados.DuvidaFeedback!),
                    new ParametroSql("input", "200", "PrimeiroNome", dados.PrimeiroNome!),
                    new ParametroSql("input", "200", "Plano", dados.Descricao!),
                    new ParametroSql("input", "200", "Sobrenome", dados.Sobrenome!),
                    new ParametroSql("input", "200", "CargoEmpresa", dados.CargoEmpresa!),
                    new ParametroSql("input", "200", "NomeEmpresa", dados.NomeEmpresa!),
                    new ParametroSql("input", "200", "Celular", dados.Celular!),
                    new ParametroSql("output", "500", "Resp", 0),
                };

                return contexto.ExecutaComandoRetornoString("[SpOperacional045SalvarFeedbackLandingPage]", list);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public List<FormModel> CarregaFormulario()
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                List<FormModel> dados = new List<FormModel>();

                List<ParametroSql> list = new()
                {

                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("[SpConsulta022RetornaDadosFormulario]", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        dados.Add(new()
                        {
                            Descricao = It["Plano"].ToString()!,
                            NomeEmpresa = It["NomeEmpresa"].ToString()!,
                            PrimeiroNome = It["PrimeiroNome"].ToString()!,
                            Sobrenome = It["Sobrenome"].ToString()!,
                            Celular = It["Celular"].ToString()!,
                            Valor = It["Valor"].ToString()!,
                            Email = It["Email"].ToString()!,
                            DuvidaFeedback = It["DuvidaFeedback"].ToString()!,
                            CargoEmpresa = It["CargoEmpresa"].ToString()!,

                        });
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

    }


}



