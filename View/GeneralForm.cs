﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ValumeFigyre;

namespace View
{
    public partial class GeneralForm : Form, IAddObjectDelegate
    {
        private IValumeFigyre _figure;

        public GeneralForm()
        {
            InitializeComponent();
        }

        private List<IValumeFigyre> ListFigure = new List<IValumeFigyre>();

        private void AddFigyre_Click(object sender, EventArgs e)
        {
            AddOrModify AddFigure = new AddOrModify(false,null);
            AddFigure.Delegate = this;
            AddFigure.FormClosed += (obj, arg) =>
            {
                if (_figure != null)
                {
                    ListFigure.Add(_figure);
                    _figure = null;
                }
                Grid.Rows.Clear();
                foreach(var figure in ListFigure)
                {
                    Grid.Rows.Add(figure.TypeFigyre, figure.Valume);
                }

            };
            AddFigure.ShowDialog();
        }

        private void RemoveFigyre_Click(object sender, EventArgs e)
        {
            if (Grid.CurrentRow != null)
            {
                try
                {
                    ListFigure.RemoveAt(Grid.CurrentRow.Index);
                    Grid.Rows.Remove(Grid.CurrentRow);
                }
                catch (System.InvalidOperationException)  { }
            }
        }

        public void DidFinish(IValumeFigyre figure)
        {
            _figure = figure;
        }

        private void Random_Click(object sender, EventArgs e)
        {
#if DEBUG
            Random random = new Random();
            //double randomValue;
            int maxRandom;
            int maxGridSize;
            maxRandom = 10;
            maxGridSize = 10;

            for (int i = 0; i < maxGridSize; i++)
            {
                switch (random.Next(0, 3))
                {
                    case 0:
                        {
                            Box BoxVolume = new Box(height: random.NextDouble() + random.Next(0, maxRandom),
                                width: random.NextDouble() + random.Next(0, maxRandom),
                                deep: random.NextDouble() + random.Next(0, maxRandom));
                            ListFigure.Add(BoxVolume);
                            break;
                        }
                    case 1:
                        {
                            Sphear SphearVolume = new Sphear(radius: random.NextDouble() + random.Next(0, maxRandom));
                            ListFigure.Add(SphearVolume);
                            break;
                        }
                    case 2:
                        {
                            Pyramid PyramidVolume = new Pyramid(area: random.NextDouble() + random.Next(0, maxRandom),
                                height: random.NextDouble() + random.Next(0, maxRandom));
                            ListFigure.Add(PyramidVolume);
                            break;
                        }
                    default:
                        break;
                }

            }
            Grid.Rows.Clear();
            foreach (var figure in ListFigure)
            {
                Grid.Rows.Add(figure.TypeFigyre, figure.Valume);
            }
#endif
        }
        /// <summary>
        /// сохранение данных таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                DataSet ds = new DataSet(); // создаем пока что пустой кэш данных
                DataTable dt = new DataTable(); // создаем пока что пустую таблицу данных
                dt.TableName = "Figures"; // название таблицы
                dt.Columns.Add("Figure"); // название колонок
                dt.Columns.Add("Volume");
                dt.Columns.Add("Width");
                dt.Columns.Add("Height");
                dt.Columns.Add("Deep");
                ds.Tables.Add(dt); //в ds создается таблица, с названием и колонками, созданными выше
                /*foreach (DataGridViewRow r in Grid.Rows) // пока в Grid есть строки
                {
                    DataRow row = ds.Tables["Figures"].NewRow(); // создаем новую строку в таблице, занесенной в ds
                    row["Figure"] = r.Cells[0].Value;  //в столбец этой строки заносим данные из первого столбца dataGridView1
                    row["Volume"] = r.Cells[1].Value; // то же самое со вторыми столбцами
                    ds.Tables["Figures"].Rows.Add(row); //добавление всей этой строки в таблицу ds.
                }*/
                foreach(var fig in ListFigure)
                {
                    DataRow row = ds.Tables["Figures"].NewRow();
                    row["Figure"] = fig.TypeFigyre;  //в столбец этой строки заносим данные из первого столбца dataGridView1
                    row["Volume"] = fig.Valume; // то же самое со вторыми столбцами
                    if (fig.TypeFigyre=="Parallepiped")
                    {
                        row["Width"] = fig.Parametr[0];
                        row["Height"] = fig.Parametr[1];
                        row["Deep"] = fig.Parametr[2];
                    }
                    else if(fig.TypeFigyre == "Sphear")
                    {
                        row["Width"] = fig.Parametr[0];
                        row["Height"] = "";
                        row["Deep"] = "";
                    }
                    else
                    {
                        row["Width"] = fig.Parametr[0];
                        row["Height"] = fig.Parametr[1];
                        row["Deep"] = "";
                    }
                    ds.Tables["Figures"].Rows.Add(row);
                }
                saveDialog.ShowDialog();
                ds.WriteXml(saveDialog.FileName);
                MessageBox.Show("Vol file successfully saved.", "Complete");
            }
            catch
            {
                MessageBox.Show("Unable to save file Vol", "Error");
            }
        }

        private void Open_Click(object sender, EventArgs e)
        {
            
                openDialog.ShowDialog();
                if ((File.Exists(openDialog.FileName)) && (openDialog.FileName.Length !=0)) // если существует данный файл
                {
                    DataSet ds = new DataSet(); // создаем новый пустой кэш данных
                    ds.ReadXml(openDialog.FileName); // записываем в него XML-данные из файла

                    foreach (DataRow item in ds.Tables["Figures"].Rows)
                    {
                    int n = Grid.Rows.Add(); // добавляем новую сроку в dataGridView1
                    Grid.Rows[n].Cells[0].Value = item["Figure"]; // заносим в первый столбец созданной строки данные из первого столбца таблицы ds.
                    Grid.Rows[n].Cells[1].Value = item["Volume"]; // то же самое со вторым столбцом
                    double[] dparametrs = { };
                    if (item["Figure"].ToString()=="Parallepiped")
                    {
                        dparametrs[0] = Convert.ToDouble(item["Width"]);
                        dparametrs[1] = Convert.ToDouble(item["Height"]);
                        dparametrs[2] = Convert.ToDouble(item["Deep"]);

                        //ListFigure.AddRange(item["Figure"], item["Volume"], dparametrs);
                    }
                    else if (item["Figure"].ToString() == "Sphear")
                    {
                        dparametrs[0] = Convert.ToDouble(item["Width"]);
                    }
                    else
                    {
                        dparametrs[0] = Convert.ToDouble(item["Width"]);
                        dparametrs[1] = Convert.ToDouble(item["Height"]);
                    }

                }
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            if (Grid.Rows.Count > 0)
            {
                Grid.Rows.Clear();
                ListFigure.Clear();
            }
            else
            {
                MessageBox.Show("Table is empty", "Error");
            }
        }

        private DataGridViewCellEventArgs _e;
        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            _e = e;
            Modify.Enabled = true;
            TypeFigure.Text = ListFigure[e.RowIndex].TypeFigyre;
            if (ListFigure[e.RowIndex].TypeFigyre == "Sphear")
            {
                XParametr.Text = ListFigure[e.RowIndex].Parametr[0].ToString();
                YParametr.Text = "";
                ZParametr.Text = "";
            }
            else if (ListFigure[e.RowIndex].TypeFigyre == "Parallepiped")
            {
                XParametr.Text = ListFigure[e.RowIndex].Parametr[0].ToString();
                YParametr.Text = ListFigure[e.RowIndex].Parametr[1].ToString();
                ZParametr.Text = ListFigure[e.RowIndex].Parametr[2].ToString();
            }
            else
            {
                XParametr.Text = ListFigure[e.RowIndex].Parametr[0].ToString();
                YParametr.Text = ListFigure[e.RowIndex].Parametr[1].ToString();
                ZParametr.Text = "";
            }
        }

        private void Modify_Click(object sender, EventArgs e)
        {
            int index = _e.RowIndex;
            AddOrModify ModifyFigure = new AddOrModify(false, ListFigure[index]);
            ModifyFigure.Delegate = this;
            ModifyFigure.FormClosed += (obj, arg) =>
            {
                if (_figure != null)
                {
                    ListFigure.RemoveAt(index);
                    ListFigure.Add(_figure);
                    Grid.Rows.RemoveAt(index);
                    Grid.Rows.Insert(index, _figure.TypeFigyre, _figure.Valume);
                    _figure = null;
                }    
            };
            ModifyFigure.ShowDialog();
        }
    }
}
