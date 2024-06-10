using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using PdfiumViewer;
using SGEServer.Data;
using SGEServer.Models;
using PdfiumViewer;
using PdfSharpCore.Pdf;

namespace SGEServer.Controllers
{
    public class ImpressaoController
    {
        public string SalvarImpressora(int CodEmpresa, int CodUsuario, string SelectedPrinter)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa),
                    new ParametroSql("input", "", "CodUsuario", CodUsuario),
                    new ParametroSql("input", "", "NomeImpressora", SelectedPrinter),
                    new ParametroSql("output", "500", "Resp", 0),
                };

                return contexto.ExecutaComandoRetornoString("[SpOperacional043CadastrarImpressora]", list);
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public ImpressaoModel CarregaImpressoraCadastrada(int CodEmpresa, int CodUsuario)
        {
            try
            {
                var contexto = new DataBase(InfoConnection.DataConect);
                ImpressaoModel dados = new ImpressaoModel();

                List<ParametroSql> list = new()
                {
                    new ParametroSql("input", "200", "CodUsuario", CodUsuario),
                    new ParametroSql("input", "", "CodEmpresa", CodEmpresa)
                };

                DataTable Dt = contexto.ExecutaComandoRetornoData("[SpConsulta021RetornaImpressoraCadastradasNoUsuario]", list);

                if (Dt != null)
                {
                    foreach (DataRow It in Dt.Rows)
                    {
                        dados.NomeImpressora = It["NomeImpressora"].ToString();
                    }
                }

                return dados;
            }
            catch (Exception exp)
            {
                throw new Exception($"Erro:! {exp.Message}");
            }
        }

        public static async Task<byte[]> ImpressoraQL800(string CodBarras, string Item, int CodProduto, decimal ValorTotal, int QuantidadeEtiquetas, int QuantidadeUnitario)
        {
            try
            {
                // Dados do produto
                string codigoBarras = CodBarras;
                string nomeProduto = Item;
                string codigoProduto = "CodProduto:" + CodProduto;
                string preco = "R$" + ValorTotal.ToString("F2");

                // Gerar a imagem do código de barras
                var barcodeImage = GerarCodigoBarras(codigoBarras);

                // Criação do documento PDF
                PdfSharpCore.Pdf.PdfDocument document = new PdfSharpCore.Pdf.PdfDocument();
                document.Info.Title = "Etiqueta Simulada";

                // Criação da página
                PdfSharpCore.Pdf.PdfPage page = document.AddPage();
                page.Width = XUnit.FromMillimeter(50); // Largura da etiqueta estreita
                page.Height = XUnit.FromMillimeter(30); // Altura da etiqueta

                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new XFont("Arial", 8, XFontStyle.Regular);
                XFont fontBold = new XFont("Arial", 10, XFontStyle.Bold);

                // Desenhar a imagem do código de barras
                using (var ms = new MemoryStream(barcodeImage))
                {
                    XImage xImage = XImage.FromStream(() => new MemoryStream(barcodeImage));
                    gfx.DrawImage(xImage, new XRect(0, 10, page.Width, 15)); // Ajuste a posição e o tamanho conforme necessário
                }

                // Desenhar informações CodProduto
                gfx.DrawString(codigoProduto.ToString(), font, XBrushes.Black, new XPoint(40, 35));

                // Dividir o nomeProduto em várias linhas
                var nomeProdutoLinhas = QuebrarTexto(nomeProduto, font, gfx, page.Width - 10);
                for (int i = 0; i < nomeProdutoLinhas.Length; i++)
                {
                    gfx.DrawString(nomeProdutoLinhas[i], font, XBrushes.Black, new XRect(5, 40 + i * 10, page.Width, page.Height), XStringFormats.TopLeft);
                }

                gfx.DrawString("Preço: " + preco, fontBold, XBrushes.Black, new XRect(5, 60 + nomeProdutoLinhas.Length * 10, page.Width, page.Height), XStringFormats.TopLeft);

                // Salvar o documento PDF em memória
                using (MemoryStream stream = new MemoryStream())
                {
                    document.Save(stream, false);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao gerar o PDF: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                throw;
            }
        }

        public static async Task ImpressoraNormal(string CodBarras, string Item, int CodProduto, decimal ValorTotal, int QuantidadeEtiquetas, int QuantidadeUnitario, string SelectedPrinter)
        {
            try
            {
                // Dados do produto
                string codigoBarras = CodBarras;
                string nomeProduto = Item;
                string codigoProduto = "CodProduto: " + CodProduto;
                string preco = "R$" + ValorTotal.ToString("F2");

                // Gerar a imagem do código de barras
                var barcodeImage = GerarCodigoBarras(codigoBarras);

                // Criação do documento PDF
                PdfSharpCore.Pdf.PdfDocument document = new PdfSharpCore.Pdf.PdfDocument();
                document.Info.Title = "Etiquetas";

                // Dimensões da etiqueta e da página
                double etiquetaLargura = XUnit.FromMillimeter(50); // em mm
                double etiquetaAltura = XUnit.FromMillimeter(32); // em mm
                double margemEsquerda = XUnit.FromMillimeter(5); // em mm
                double margemSuperior = XUnit.FromMillimeter(5); // em mm
                double margemHorizontalEntreEtiquetas = XUnit.FromMillimeter(0); // Espaço horizontal entre as etiquetas, em mm
                double margemVerticalEntreEtiquetas = XUnit.FromMillimeter(0); // Espaço vertical entre as etiquetas, em mm
                int etiquetasPorLinha = 4;
                int linhasPorPagina = 9;

                int totalEtiquetas = QuantidadeEtiquetas;
                int identificador = 1;

                while (totalEtiquetas > 0)
                {
                    PdfSharpCore.Pdf.PdfPage page = document.AddPage();
                    page.Width = XUnit.FromMillimeter(210); // Largura da página A4
                    page.Height = XUnit.FromMillimeter(297); // Altura da página A4

                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XFont font = new XFont("Arial", 8, XFontStyle.Regular);
                    XFont fontBold = new XFont("Arial", 10, XFontStyle.Bold);
                    XPen pen = new XPen(XColors.Black, 0.5); // Caneta para desenhar as bordas

                    for (int linha = 0; linha < linhasPorPagina && totalEtiquetas > 0; linha++)
                    {
                        for (int coluna = 0; coluna < etiquetasPorLinha && totalEtiquetas > 0; coluna++)
                        {
                            double x = margemEsquerda + coluna * (etiquetaLargura + margemHorizontalEntreEtiquetas);
                            double y = margemSuperior + linha * (etiquetaAltura + margemVerticalEntreEtiquetas);

                            // Desenhar a borda da etiqueta
                            gfx.DrawRectangle(pen, x, y, etiquetaLargura, etiquetaAltura);

                            // Adicionar o identificador único
                            gfx.DrawString($"{identificador}", font, XBrushes.Black, new XRect(x + XUnit.FromMillimeter(2), y + XUnit.FromMillimeter(2), etiquetaLargura, XUnit.FromMillimeter(10)), XStringFormats.TopLeft);
                            identificador++;

                            // Desenhar a imagem do código de barras
                            using (var ms = new MemoryStream(barcodeImage))
                            {
                                XImage xImage = XImage.FromStream(() => new MemoryStream(barcodeImage));
                                gfx.DrawImage(xImage, new XRect(x + XUnit.FromMillimeter(5), y + XUnit.FromMillimeter(2), etiquetaLargura - XUnit.FromMillimeter(10), XUnit.FromMillimeter(12))); // Ajuste a posição e o tamanho conforme necessário
                            }

                            // Desenhar informações CodProduto
                            gfx.DrawString(codigoProduto, font, XBrushes.Black, new XRect(x + XUnit.FromMillimeter(15), y + XUnit.FromMillimeter(15), etiquetaLargura - XUnit.FromMillimeter(10), XUnit.FromMillimeter(10)), XStringFormats.TopLeft);

                            // Dividir o nomeProduto em várias linhas
                            var nomeProdutoLinhas = QuebrarTexto(nomeProduto, font, gfx, etiquetaLargura - XUnit.FromMillimeter(10));
                            for (int i = 0; i < nomeProdutoLinhas.Length; i++)
                            {
                                gfx.DrawString(nomeProdutoLinhas[i], font, XBrushes.Black, new XRect(x + XUnit.FromMillimeter(5), y + XUnit.FromMillimeter(18) + i * XUnit.FromMillimeter(10), etiquetaLargura - XUnit.FromMillimeter(10), XUnit.FromMillimeter(10)), XStringFormats.TopLeft);
                            }

                            gfx.DrawString("Preço: " + preco, fontBold, XBrushes.Black, new XRect(x + XUnit.FromMillimeter(5), y + XUnit.FromMillimeter(17) + nomeProdutoLinhas.Length * XUnit.FromMillimeter(10), etiquetaLargura - XUnit.FromMillimeter(10), XUnit.FromMillimeter(10)), XStringFormats.TopLeft);

                            totalEtiquetas--;
                        }
                    }
                }

                // Salvar o documento PDF em memória
                using (MemoryStream stream = new MemoryStream())
                {
                    document.Save(stream, false);

                    // Enviar para a impressora selecionada
                    PrintDocument printDocument = new PrintDocument();
                    printDocument.PrinterSettings.PrinterName = SelectedPrinter;

                    printDocument.PrintPage += (sender, e) =>
                    {
                        // Converter o PDF em uma imagem usando PdfiumViewer
                        stream.Position = 0;
                        using (var pdfDocument = PdfiumViewer.PdfDocument.Load(stream))
                        {
                            var pageImage = pdfDocument.Render(0, e.PageBounds.Width, e.PageBounds.Height, true);
                            e.Graphics.DrawImage(pageImage, e.PageBounds);
                        }
                    };

                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao gerar o PDF: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                throw;
            }
        }

        private static byte[] GerarCodigoBarras(string codigoBarras)
        {
            try
            {
                var barcodeWriter = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = 300,
                        Height = 100
                    }
                };

                var pixelData = barcodeWriter.Write(codigoBarras);

                using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
                {
                    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                    try
                    {
                        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }

                    using (var stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao gerar o código de barras: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                throw;
            }
        }

        private static string[] QuebrarTexto(string texto, XFont fonte, XGraphics gfx, double larguraMaxima)
        {
            var palavras = texto.Split(' ');
            var linhas = new List<string>();
            var linhaAtual = "";

            foreach (var palavra in palavras)
            {
                var linhaTeste = linhaAtual.Length == 0 ? palavra : linhaAtual + " " + palavra;
                var tamanho = gfx.MeasureString(linhaTeste, fonte);
                if (tamanho.Width <= larguraMaxima)
                {
                    linhaAtual = linhaTeste;
                }
                else
                {
                    linhas.Add(linhaAtual);
                    linhaAtual = palavra;
                }
            }

            if (linhaAtual.Length > 0)
            {
                linhas.Add(linhaAtual);
            }

            return linhas.ToArray();
        }
    }
}
