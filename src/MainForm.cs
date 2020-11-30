using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FsDedunderator
{
    public class MainForm : Form
    {
        private readonly IContainer components;
        private readonly BindingList<CrawlData> crawlSourceList;
        private readonly BindingSource crawlSourceBindingSource;
        private readonly TableLayoutPanel outerTableLayoutPanel;
        
        public MainForm() {
            components = new Container();
            crawlSourceList = new BindingList<CrawlData>();
            SuspendLayout();
        }
    }
}