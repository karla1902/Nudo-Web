using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Proyecto_Nudo_Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Proyecto_Nudo_Web.Controllers
{
    public class Archivo : Controller
    {

        private readonly IWebHostEnvironment _host;

        public Archivo(IWebHostEnvironment host)
        {
            _host = host;
        }

        public IActionResult Index()
        {
            ViewBag.Tabla = 0;
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormCollection collection, int idColumna, string selecciones, string rutas)
        {
            var files = Request.Form.Files;
            string filepathS = string.Empty;
            //OBTENER SELECCIONES DE CABECERAS**********************

            if (selecciones == null || selecciones == "" || selecciones == string.Empty)
            {
                selecciones = "#";
            }
            var cabecerasSeleccionadas = selecciones.Trim().Split("#");
            selecciones = selecciones.Substring(1, selecciones.Length - 1);

            //OBTENER RUTAS DE ARCHIVOS

            if (rutas == null || rutas == "" || rutas == string.Empty)
            {
                rutas = "#";
                rutas = rutas.Substring(1, rutas.Length - 1);
            }
            var rutasArchivos = rutas.Trim().Split("#");
            //rutas = rutas.Substring(1, rutas.Length - 1);
            rutasArchivos = rutasArchivos.Take(rutasArchivos.Length - 1).ToArray();


            string[,] dataTabla = new string[files.Count, 100];
            string[,] indice = new string[files.Count, 10];

            int cantidadArchivos = files.Count;

            ViewBag.cantidadArchivos = cantidadArchivos;

            // int contadorForeach = 0;

            List<List<string>> encabezados = new List<List<string>>();
            if (files.Count() > 0)
            {
                encabezados = (ObtenerEncabezados(files, filepathS, cabecerasSeleccionadas, cantidadArchivos, rutasArchivos));
                ViewBag.Tabla = 0;
            }

            if (rutas != "#" && rutas != "")
            {
                ConcatenarArchivos(rutasArchivos, cabecerasSeleccionadas, files, filepathS, cantidadArchivos);
            }

            //TODA LA INFO
            //var dato = dataTabla;
            //List<string> infoDatos = new List<string>();

            //for (int i = 0; i < 100; i++)
            //{
            //    string valor = string.Empty;

            //    for (int j = 0; j < cantidadArchivos; j++)
            //    {
            //        valor = valor + ";";
            //        valor = valor + dato[j, i];
            //    }

            //    //crear if validando si no es nulo, mandar a un view bag y validarlo en front end
            //    if (valor != null || valor != "" || valor != string.Empty)
            //    {
            //        valor = valor.Substring(1, valor.Length - 1);
            //        infoDatos.Add(valor);
            //    }
            //}


            List<string> datosFinal = new List<string>();

            //foreach (var item in infoDatos)
            //{
            //    if (item != ";")
            //    {
            //        datosFinal.Add(item);
            //    }
            //}



            //llamar view bag 
            ViewBag.data = datosFinal;
            ViewBag.columna = indice;
            ViewBag.Encabezados = encabezados;

            return View();
        }

        private List<List<string>> ObtenerEncabezados(IFormFileCollection files, string filepathS, string[] cabecerasSeleccionadas, int cantidadArchivos, string[] rutasArchivos)
        {
            List<string> nombreArchivo = new List<string>();
            List<List<string>> encabezados = new List<List<string>>();
            if (files != null)
            {
                foreach (var item in files)
                {
                    List<string> encabezadosArchivo = new List<string>();

                    var filename = item.FileName;
                    string nombreArch = filename;


                    string filePath = _host.ContentRootPath + @"/wwwroot/temp" + $@"/{filename}";

                    //elimina archvios si existen
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    //guardar archivo
                    using (FileStream fs = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(fs);
                        fs.Flush();
                    }

                    filepathS = filePath + "#" + filepathS;
                    //rutas = rutas+filePath + "#";
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        if (Path.GetExtension(filePath) == ".csv")
                        {
                            using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                            {
                                //1.use the reader methods
                                do
                                {
                                    while (reader.Read())
                                    {
                                        //reader.getdouble(0);
                                    }
                                } while (reader.NextResult());

                                IExcelDataReader archivos = reader;

                                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                {
                                    ConfigureDataTable = (data) => new ExcelDataTableConfiguration()
                                    {
                                        UseHeaderRow = true
                                    }
                                });

                                DataTableCollection table = result.Tables;
                                DataTable resulttable = table[0];

                                foreach (DataColumn archivo in resulttable.Columns)
                                {
                                    encabezadosArchivo.Add(archivo.ColumnName);
                                }
                            }
                        }
                    }

                    encabezados.Add(encabezadosArchivo);
                    nombreArchivo.Add(filename);
                    ViewBag.nombreArchivo = nombreArchivo;
                }
            }

            else
            {
                foreach (var item in rutasArchivos)
                {

                    List<string> encabezadosArchivo = new List<string>();
                    var filename = item;

                    string filePath = filename;

                    filepathS = filepathS + filePath + "#";
                    //rutas = rutas+filePath + "#";
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        if (Path.GetExtension(filePath) == ".csv")
                        {
                            using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                            {
                                //1.use the reader methods
                                do
                                {
                                    while (reader.Read())
                                    {
                                        //reader.getdouble(0);
                                    }
                                } while (reader.NextResult());

                                IExcelDataReader archivos = reader;

                                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                {
                                    ConfigureDataTable = (data) => new ExcelDataTableConfiguration()
                                    {
                                        UseHeaderRow = true
                                    }
                                });

                                DataTableCollection table = result.Tables;
                                DataTable resulttable = table[0];

                                foreach (DataColumn archivo in resulttable.Columns)
                                {
                                    encabezadosArchivo.Add(archivo.ColumnName);

                                }
                            }
                        }
                    }



                    encabezados.Add(encabezadosArchivo);


                }
            }
            ViewBag.filePaths = filepathS;

            return encabezados;
        }


        ////PROCESO CONCATENACION
        //if (selecciones != "#" && selecciones != "" && rutasArchivos.Count() > 1)
        //{
        //    using (var stream = System.IO.File.Open(rutasArchivos[1], FileMode.Open, FileAccess.Read))
        //    {
        //        if (Path.GetExtension(rutasArchivos[1]) == ".csv")
        //        {
        //            using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
        //            {
        //                //1.Use the reader methods
        //                do
        //                {
        //                    while (reader.Read())
        //                    {
        //                        //reader.GetDouble(0);
        //                    }
        //                } while (reader.NextResult());

        //                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
        //                {
        //                    ConfigureDataTable = (data) => new ExcelDataTableConfiguration()
        //                    {
        //                        UseHeaderRow = true
        //                    }
        //                });

        //                DataTableCollection table = result.Tables;
        //                DataTable resultTable = table[0];

        //                foreach (DataColumn archivo in resultTable.Columns)
        //                {
        //                    //encabezadosArchivo.Add(archivo.ColumnName);
        //                    ////CONCATENAR FILAS 
        //                    //List<string> fila1 = new List<string>();
        //                    //List<string> guardarFila = new List<string>();

        //                    if (archivo.ColumnName == cabecerasSeleccionadas[1])
        //                    {
        //                        //fila1.Add(archivo.ColumnName);

        //                        //cantidadArchivos++;
        //                        //foreach (var archivoX in files.Count.ToString())
        //                        //{
        //                        //    foreach (var filas in archivoX.ToString())
        //                        //    {
        //                        //        guardarFila.Add(filas.ToString());
        //                        //        continue;
        //                        //    }
        //                        //}
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private void ConcatenarArchivos(string[] rutasArchivos, string[] cabecerasSeleccionadas, IFormFileCollection files, string filepathS, int cantidadArchivos)
        {
            /*
             * 1.- realizas proceso de lectura de archivos** 
             *>> 2.- realizas proceso de busqueda de columnas a concatenar
             *>> 3.- realizas concatenacion de archivos (tablas)
             * 4.- generas nueva tabla final con datos
             * 5.- retornas como lista de string los datos
             * 6.- envias por ViewBag los datos para la creación de la tabla en HTML
             */
            var rutaArchivo = rutasArchivos;
            var cabeceraSeleccionada = cabecerasSeleccionadas;

            DataTable tablaConcatenada = new DataTable();
            DataColumn columna;

            List<List<string>> encabezados = ObtenerEncabezados(null, filepathS, cabecerasSeleccionadas, cantidadArchivos, rutasArchivos);

            foreach (var archivo in encabezados)
            {
                foreach (var encabezado in archivo)
                {
                    columna = new DataColumn();
                    columna.DataType = Type.GetType("System.String");
                    try
                    {
                        columna.ColumnName = encabezado;
                        tablaConcatenada.Columns.Add(columna);
                    }
                    catch (Exception)
                    {
                        columna.ColumnName = encabezado;

                        int idColumna = 1;

                        foreach (DataColumn item in tablaConcatenada.Columns)
                        {
                            if (encabezado == item.ColumnName)
                            {
                                idColumna++;
                                //columna.ColumnName = encabezado + "(" + idColumna + ")";
                            }
                            else if (encabezado + "(" + idColumna + ")" == item.ColumnName)
                            {
                                idColumna++;
                            }
                        }
                        columna.ColumnName = encabezado + "(" + idColumna + ")";
                        tablaConcatenada.Columns.Add(columna);
                    }
                }
            }

            //PROCESO DE CONCATENACION *****************************************************************

            string filePath = rutasArchivos[0];

            DataRow filaArchivo;

            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                if (Path.GetExtension(filePath) == ".csv")
                {
                    using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                    {
                        //1.use the reader methods
                        do
                        {
                            while (reader.Read())
                            {
                                //reader.getdouble(0);
                            }
                        } while (reader.NextResult());

                        IExcelDataReader archivos = reader;

                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (data) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });

                        DataTableCollection table = result.Tables;
                        DataTable resulttable = table[0];

                        foreach (DataRow fila in resulttable.Rows)
                        {
                            //lista de filas de los archivos
                            List<List<string>> FilaTotal = new List<List<string>>();

                            //Almacena los datos de una fila
                            List<string> filaparcial = new List<string>();
                            for (int i = 0; i < resulttable.Columns.Count; i++)
                            {
                                filaparcial.Add(fila[i].ToString());
                            }

                            //Agrerga la fila de datos a la fila total
                            FilaTotal.Add(filaparcial);

                            //busqueda de posicion de columna seleccionada
                            int posCol = PosicionColumnaBuscada(resulttable.Columns, cabeceraSeleccionada[0]);

                            //almacena valor buscado para concatenar
                            string valorBuscado = fila[posCol].ToString();

                            //Buscar valor en otros archivos
                            int indicadorSeleccion = 0;

                            //Quita de la lista el archivo utilizado como base
                            var archivosFaltantes = rutasArchivos;
                            archivosFaltantes = archivosFaltantes.Where((source, index) => index != 0).ToArray();

                            List<DataTable> archivosDataTable = new List<DataTable>();
                            foreach (var archivo in archivosFaltantes)
                            {
                                using (var stream2 = System.IO.File.Open(archivo, FileMode.Open, FileAccess.Read))
                                {
                                    using (var reader2 = ExcelReaderFactory.CreateCsvReader(stream2))
                                    {
                                        IExcelDataReader archivos2 = reader2;

                                        var result2 = reader2.AsDataSet(new ExcelDataSetConfiguration()
                                        {
                                            ConfigureDataTable = (data) => new ExcelDataTableConfiguration()
                                            {
                                                UseHeaderRow = true
                                            }
                                        });

                                        DataTableCollection table2 = result2.Tables;
                                        DataTable resulttable2 = table2[0];
                                        archivosDataTable.Add(resulttable2);
                                    }
                                }
                                indicadorSeleccion++;
                            }

                            foreach (DataTable archivo in archivosDataTable)
                            {
                                List<string> filaparcial2 = new List<string>();
                                //localizar columna de busqueda
                                int posCol2 = PosicionColumnaBuscada(archivo.Columns, cabeceraSeleccionada[indicadorSeleccion]);

                                //bool filaEncontrada = false;
                                //int indexFila = 0;

                                var y = (from x in archivo.AsEnumerable()
                                         where x.Field<string>(cabeceraSeleccionada[indicadorSeleccion]) == valorBuscado
                                         select x).FirstOrDefault();
                                //si la var y es distinta a nula
                                if (y != null)
                                {
                                    for (int i = 0; i < archivo.Columns.Count; i++)
                                    {
                                        filaparcial2.Add(y[i].ToString());
                                    }
                                }
                                

                                FilaTotal.Add(filaparcial2);

                                //foreach (DataRow fila2 in archivo.Rows)
                                //{
                                //    if (fila2[posCol2].ToString() == valorBuscado)
                                //    {
                                //        for (int i = 0; i < archivo.Columns.Count; i++)
                                //        {
                                //            filaparcial2.Add(fila2[i].ToString());
                                //        }
                                //        fila2.Delete();
                                //        filaEncontrada = true;
                                //    }
                                //    else if (filaEncontrada == true)
                                //    {
                                //        break;
                                //    }
                                //    indexFila++;
                                //}

                                //Almacena fila parcial en fila total
                                //FilaTotal.Add(filaparcial2);
                            }

                            ////Recorre archivos faltantes
                            //foreach (var archivo in archivosFaltantes)
                            //{
                            //    indicadorSeleccion++;
                            //    List<string> filaparcial2 = new List<string>();

                            //    using (var stream2 = System.IO.File.Open(archivo, FileMode.Open, FileAccess.Read))
                            //    {
                            //        using (var reader2 = ExcelReaderFactory.CreateCsvReader(stream2))
                            //        {
                            //            IExcelDataReader archivos2 = reader2;

                            //            var result2 = reader2.AsDataSet(new ExcelDataSetConfiguration()
                            //            {
                            //                ConfigureDataTable = (data) => new ExcelDataTableConfiguration()
                            //                {
                            //                    UseHeaderRow = true
                            //                }
                            //            });

                            //            DataTableCollection table2 = result2.Tables;
                            //            DataTable resulttable2 = table2[0];

                            //            //localizar columna de busqueda
                            //            int posCol2 = PosicionColumnaBuscada(resulttable2.Columns, cabeceraSeleccionada[indicadorSeleccion]);

                            //            bool filaEncontrada = false;
                            //            foreach (DataRow fila2 in resulttable2.Rows)
                            //            {
                            //                if (fila2[posCol2].ToString() == valorBuscado)
                            //                {
                            //                    for (int i = 0; i < resulttable2.Columns.Count; i++)
                            //                    {
                            //                        filaparcial2.Add(fila2[i].ToString());
                            //                    }
                            //                    filaEncontrada = true;
                            //                }
                            //                else if (filaEncontrada == true)
                            //                {
                            //                    break;
                            //                }

                            //            }

                            //            //Almacena fila parcial en fila total
                            //            FilaTotal.Add(filaparcial2);

                            //        }
                            //    }


                            //}


                            //transformo todas las filas encontradas en una sola concatenada
                            List<string> filaConcatenada = new List<string>();
                            //FilaTotal.Reverse();
                            foreach (var filaArchivoEncontrada in FilaTotal)
                            {
                                foreach (var filaEncontrada in filaArchivoEncontrada)
                                {
                                    filaConcatenada.Add(filaEncontrada);
                                }
                            }

                            //creo la fila para la datatable
                            int posicionColumnaTablaConcatenada = 0;
                            filaArchivo = tablaConcatenada.NewRow();

                            foreach (var item in filaConcatenada)
                            {
                                filaArchivo[posicionColumnaTablaConcatenada] = item.ToString();
                                posicionColumnaTablaConcatenada++;
                            }
                            tablaConcatenada.Rows.Add(filaArchivo);


                        }
                    }
                }
            }


            List<string> TablaFinalCabeceras = new List<string>();
            List<List<string>> TablaFinalDatos = new List<List<string>>();

            //cabeceras
            foreach (DataColumn col in tablaConcatenada.Columns)
            {
                TablaFinalCabeceras.Add(col.ColumnName);
            }
            //datos
            foreach (DataRow fil in tablaConcatenada.Rows)
            {
                List<string> fila = new List<string>();
                for (int i = 0; i < tablaConcatenada.Columns.Count; i++)
                {
                    fila.Add(fil[i].ToString());
                }
                TablaFinalDatos.Add(fila);
            }

            ViewBag.EncabezadosFinal = TablaFinalCabeceras;
            ViewBag.DatosFinal = TablaFinalDatos;



            //TRANSFORMACION DE DATATABLE A TABLA HTML ***********************************************************************************************
            //string html = "<table>";
            ////add header row
            //html += "<tr>";
            //for (int i = 0; i < tablaConcatenada.Columns.Count; i++)
            //    html += "<td>" + tablaConcatenada.Columns[i].ColumnName + "</td>";
            //html += "</tr>";
            ////add rows
            //for (int i = 0; i < tablaConcatenada.Rows.Count; i++)
            //{
            //    html += "<tr>";
            //    for (int j = 0; j < tablaConcatenada.Columns.Count; j++)
            //        html += "<td>" + tablaConcatenada.Rows[i][j].ToString() + "</td>";
            //    html += "</tr>";
            //}
            //html += "</table>";

            string a = string.Empty;

            //ViewBag.TablaFinal = html.ToString();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        private int PosicionColumnaBuscada(DataColumnCollection columnas, string columnaBuscada)
        {
            bool encontrado = false;
            int posCol = 0;
            foreach (DataColumn col in columnas)
            {
                if (col.ColumnName == columnaBuscada)
                {
                    encontrado = true;
                }
                else if (encontrado == false)
                {
                    posCol++;
                }
            }

            return posCol;
        }
    }
}
