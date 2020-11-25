using DevExpress.Pdf;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Google.Cloud.Vision.V1;
using NF.A2P;
using NF.A2P.Grid;
using NF.A2P.Helper;
using NF.A2P.HelpPopUp;
using NF.Framework.Common;
using SYS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace INT
{
    public partial class M_INT_INVOICE_AP_LINER : NF.Framework.Win.A2PFormBase
    {
        #region ▶ Initialize --------

        M_INT_INVOICE_AP_LINER_D _D = null;
        FreeFormBinding _HEADER = new FreeFormBinding();
        string _noSlipInvoice = string.Empty;
        string _menuId = string.Empty;
        string _noSlipProgress = string.Empty;
        string _noSlipBL = string.Empty;
        string _fgRegType = string.Empty;
        string _fgShippingMode = string.Empty;
        string _noInvoiceAp = string.Empty;

        string _tpAirSea = string.Empty;
        string _tpBound = string.Empty;
        string[] _freightParam = null;

        string _ynAcctInfo = string.Empty;
        string _cdPartnerBillTo = string.Empty;
        string _nmPartnerBillTo = string.Empty;
        string _cdCurrency = "JPY";

        ftpUtil _ftpUtil = null;
        string _fileServerIP = string.Empty;
        string _fileServerID = string.Empty;
        string _fileServerPW = string.Empty;
        string _fileServerPort = string.Empty;

        public string _pdfFilePath = "";
        public string path = Application.StartupPath + @"/";

        List<string> _totalKeywords = new List<string>();
        Dictionary<string, double> _documentFrequency;
        Dictionary<string, string[]> _keywords;
        Dictionary<string, string> _mappingDay;

        public M_INT_INVOICE_AP_LINER()
        {
            try
            {
                InitLoad();
                InitializeComponent();
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _freightParam = new string[1];
            _freightParam[0] = "S" + "O";

            _D = new M_INT_INVOICE_AP_LINER_D();
            InitializeGrid();
            InitializeControl();
            InitializeEvent();
            InitializeFileServerConnect();
        }

        public void InitLoad()
        {
            Console.WriteLine("abc");
        
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Application.StartupPath + @"/aif-ocr-3b4468d50e67.json");

            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists) di.Create();
        }

        private void InitializeGrid()
        {
            bandedGridView1.OptionsView.ShowGroupPanel = false;
            bandedGridView1.OptionsView.ColumnAutoWidth = false;
            bandedGridView1.OptionsCustomization.AllowSort = false;
            bandedGridView1.OptionsView.ShowColumnHeaders = false;
            bandedGridView1.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDownFocused;
            bandedGridView1.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] { new DevExpress.XtraGrid.Columns.GridColumnSortInfo(Column_SeqFreight, DevExpress.Data.ColumnSortOrder.Ascending) });

            Column_TmInvoicePost.ColumnEdit = CH.SetGridMask(NF.A2P.CommonFunction.MaskType.DATE);

            repositoryItemLookUpEdit1.DataSource = CH.GetCode("INT007", true);
            repositoryItemLookUpEdit2.DataSource = CH.GetCode("INT002", true);
            repositoryItemLookUpEdit3.DataSource = CH.GetCode("INT010", true);
            repositoryItemLookUpEdit4.DataSource = CH.GetCode("MAS001", true);

            aGridHelper.SetDecimalPoint(aGrid_Freight, new string[] { "QT_UNIT" }, 3);
            aGridHelper.SetDecimalPoint(aGrid_Freight, new string[] { "RT_FREIGHT_VAT" }, 1);
            aGridHelper.SetDecimalPoint(aGrid_Freight, new string[] { "RT_XCRT", "RT_UNIT" }, 4);
            aGridHelper.SetDecimalPoint(aGrid_Freight, new string[] { "AM_FREIGHT_COST", "AM_FREIGHT_VAT_COST", "AM_FREIGHT_SUM_COST" }, 2);

            bandedGridView1.OptionsView.ShowFooter = true;
            bandedGridView1.Columns["AM_FREIGHT_COST"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, bandedGridView1.Columns["AM_FREIGHT_COST"].FieldName, "{0:#,##0.00}");
            bandedGridView1.Columns["AM_FREIGHT_VAT_COST"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"].FieldName, "{0:#,##0.00}");
            bandedGridView1.Columns["AM_FREIGHT_SUM_COST"].Summary.Add(DevExpress.Data.SummaryItemType.Sum, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"].FieldName, "{0:#,##0.00}");

            DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit repositoryItemLookUpEdit5 = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            repositoryItemLookUpEdit5 = CH.SetGridLookUpItem(CH.GetCode("MAS_FREIGHT", _freightParam), new string[] { "CODE", "NAME", "NAME1", "NAME2" }, new string[] { "FREIGHT CODE", "FREIGHT NAME", "TAX", "UNIT" });
            repositoryItemLookUpEdit5.QueryCloseUp += RepositoryItemLookUpEdit5_QueryCloseUp;

            Column_CdFreight.ColumnEdit = repositoryItemLookUpEdit5;
            repositoryItemLookUpEdit3.Closed += RepositoryItemLookUpEdit3_Closed;
        }

        private void InitializeControl()
        {
            DataTable dtShipMode = new DataView(CH.GetCode("INT003"), "CODE IN ('CONSOL', 'FCL', 'LCL')", "CODE", DataViewRowState.CurrentRows).ToTable();

            SetControl ctr = new SetControl();
            if (Global.BizCode == "TYO")
            {
                ctr.SetCombobox(aLookUpEdit_ShipMode, CH.GetCode("INT003"));
                ctr.SetCombobox(look_Package, CH.GetCode("MAS_UNIT"));

                aLabel91.Text = "Ref No.";
                aTextEdit_VendorMBLNo.Visible = true;
                aLabel28.Visible = true;
                aPanel4.Visible = true;
            }
            else
            {
                ctr.SetCombobox(aLookUpEdit_ShipMode, dtShipMode);

                aLabel91.Text = "Portal No.";
                aTextEdit_VendorMBLNo.Visible = false;
                aLabel28.Visible = false;
                aPanel4.Visible = false;
            }
            ctr.SetCombobox(aLookUpEdit_Currency, CH.GetCode("MAS001", true));

            DataRow rowOffice = MasterHelper.GetOffice(Global.BizCode);
            if (rowOffice != null)
                aLookUpEdit_Currency.EditValue = A.GetString(rowOffice["CD_CURRENCY"]);

            aDateEdit_TM_INVOICE_RECEIVED.Text = A.GetToday;
            aLabel_Duplicate.Visible = false;

            aLabel_FileNo.Visible = true;
            aLabel_FileYes.Visible = false;
            aLabel_MappingNo.Visible = true;
            aLabel_MappingYes.Visible = false;
            aLabel_DataNo.Visible = true;
            aLabel_DataYes.Visible = false;

            _noSlipInvoice = string.Empty;
            _pdfFilePath = string.Empty;

            aTextEdit_filename.Text = string.Empty;
            aTextEdit_VendorRefNo.Text = string.Empty;
            aTextEdit_VendorInvNo.Text = string.Empty;
            aTextEdit_VendorMBLNo.Text = string.Empty;
            aDateEdit_VendorDueDate.Text = string.Empty;
            aDateEdit_VendorReceivedDate.Text = string.Empty;
            aNumericText_TotalAmount.Text = "0.00";

            if (Global.BizCode == "MIL" || Global.BizCode == "PAR" || Global.BizCode == "FRA" || Global.BizCode == "HAM")
            {
                aDateEdit_InvoiceDate.Requiredfield = false;
                aDateEdit_InvoiceDate.ReadOnly = true;
                aDateEdit_DueDate.ReadOnly = true;
            }

            InitializeHeader();
        }

        private void InitializeHeader()
        {
            DataSet ds = _D.Search(new object[] { Global.FirmCode, Global.BizCode, "#$#$", "M" });
            _HEADER.SetBinding(ds.Tables[0], tableLayoutPanel2);
            _HEADER.ClearAndNewRow();

            aGrid_MBLContainer.Binding(ds.Tables[1]);
            aGrid_Freight.Binding(ds.Tables[2]);
        }

        private void InitializeEvent()
        {
            aButton_OrderPopUp.Click += AButton_OrderPopUp_Click;

            Button_ADD.Click += Button_ADD_Click;
            Button_DEL.Click += Button_DEL_Click;
            Button_ADD.GotFocus += Button_ADD_GotFocus;
            //aButton_Container.Click += AButton_Container_Click;
            //aButton_ARCreate.Click += AButton_ARCreate_Click;
            //aButton_Copy.Click += AButton_Copy_Click;
            //aButton_Tariff.Click += AButton_Tariff_Click;
            aButton_Detect.Click += AButton_Detect_Click;
            aButton_Search.Click += AButton_Search_Click;
            aButton_Reflact.Click += AButton_Reflact_Click;
            aButton_Save.Click += AButton_Save_Click;
            aButton_AP_Approval.Click += AButton_AP_Approval_Click;

            aCodeText_Vendor.Select();
            aCodeText_Vendor.AfterCodeValueChanged += ACodeText_Vendor_AfterCodeValueChanged;

            aLookUpEdit_Currency.EditValueChanged += aLookUpEdit_Currency_EditValueChanged;

            aTextEdit_InvoiceNo.LostFocus += ATextEdit_InvoiceNo_LostFocus;
            aTextEdit_ShipmentNo.DoubleClick += ATextEdit_ShipmentNo_DoubleClick;
            aTextEdit_RequestNo.DoubleClick += ATextEdit_RequestNo_DoubleClick;
            aTextEdit_AttachInvFile.DoubleClick += ATextEdit_AttachInvFile_DoubleClick;
            aTextEdit_InvoiceNo.EditValueChanged += ATextEdit_InvoiceNo_EditValueChanged;

            aDateEdit_DueDate.EditValueChanged += ADateEdit_DueDate_EditValueChanged;
            aDateEdit_TM_INVOICE_RECEIVED.EditValueChanged += ADateEdit_TM_INVOICE_RECEIVED_EditValueChanged;
            aDateEdit_InvoiceDate.EditValueChanged += ADateEdit_InvoiceDate_EditValueChanged;

            bandedGridView1.InitNewRow += GridView1_InitNewRow;
            bandedGridView1.CellValueChanged += BandedGridView1_CellValueChanged;
            bandedGridView1.ShowingEditor += BandedGridView1_ShowingEditor;

            pdfViewer1.DocumentChanged += PdfViewer2_DocumentChanged;
            pdfViewer1.DragEnter += PdfViewer1_DragEnter;
            pdfViewer1.DragDrop += PdfViewer1_DragDrop;
        }

        private void InitializeFileServerConnect()
        {
            string Path = AppDomain.CurrentDomain.BaseDirectory + @"Setting.ini";

            SYS.IniFile inifile = new SYS.IniFile();

            _fileServerIP = inifile.IniReadValue("FileServer", "IP", Path);
            _fileServerID = inifile.IniReadValue("FileServer", "ID", Path);
            _fileServerPW = inifile.IniReadValue("FileServer", "PW", Path);
            _fileServerPort = inifile.IniReadValue("FileServer", "PORT", Path);


            if (_fileServerIP == string.Empty)
            {
                //default 
                inifile.IniWriteValue("FileServer", "IP", "211.52.110.90", Path);
                _fileServerIP = inifile.IniReadValue("FileServer", "IP", Path);
            }

            if (_fileServerID == string.Empty)
            {
                //default 
                inifile.IniWriteValue("FileServer", "ID", "administrator", Path);
                _fileServerID = inifile.IniReadValue("FileServer", "ID", Path);
            }

            if (_fileServerPW == string.Empty)
            {
                //default 
                inifile.IniWriteValue("FileServer", "PW", "aif@123$", Path);
                _fileServerPW = inifile.IniReadValue("FileServer", "PW", Path);
            }

            if (_fileServerPort == string.Empty)
            {
                //default 
                inifile.IniWriteValue("FileServer", "PORT", "21", Path);
                _fileServerPort = inifile.IniReadValue("FileServer", "PORT", Path);
            }

            _ftpUtil = new ftpUtil(_fileServerIP, _fileServerID, _fileServerPW, _fileServerPort);
        }

        #endregion

        #region ▶ Main button Event -

        public override void OnView()
        {
            try
            {

            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        public override void OnInsert()
        {
            try
            {
                InitializeHeader();
                MenuKey = string.Empty;
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        public override void OnDelete()
        {
            try
            {
                if (ShowMessageBoxA("Do you really want to delete this?", MessageType.Question) != DialogResult.Yes)
                    return;

                if (_HEADER.CurrentRow.RowState == DataRowState.Added)
                {
                    InitializeHeader();
                    return;
                }

                object[] obj = new object[] { Global.FirmCode,
                                              A.GetString(_HEADER.CurrentRow["CD_BIZ"]),
                                              A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE"]) };

                /*
                 * 2016.11.10 - KJH
                 * 삭제 전 체크
                 * P : 정산이 되었으면 삭제 불가
                 * C : CLOSE 상태 삭제 불가
                 * O : 삭제 가능
                 */
                string result = DBHelper.ExecuteScalar("AP_INT_INVOICE_CHECK_S", obj) as string;

                if (result == "P")
                {
                    ShowMessageBoxA("This Invoice has been paid.", MessageType.Warning);
                    return;
                }
                else if (result == "C")
                {
                    ShowMessageBoxA("This Invoice was closed.", MessageType.Warning);
                    return;
                }

                if (_D.Delete(obj))
                {
                    ShowMessageBoxA("It was successfully deleted.", MessageType.Information);
                    InitializeHeader();
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        public override void OnSave()
        {
            try
            {
                if (!BeforeSave()) return;

                if (!string.IsNullOrEmpty(_noInvoiceAp))
                {
                    if (ShowMessageBoxA("AP was already Issued.\n\r Do you want to create AP?", MessageType.Question) == DialogResult.Yes)
                    {
                        DoSave();
                    }
                }
                else
                {
                    DoSave();
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        public override void OnModuleFile()
        {
            base.OnModuleFile();

            try
            {
                // FG_MEMO, NO_SLIP_REL
                string _noSlipRel = A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE"]);
                if (_noSlipRel != string.Empty)
                {
                    object[] obj = {
                        "INV",
                        _noSlipRel
                    };
                    POPUP_FILE pop = new POPUP_FILE(obj);
                    pop.Show();
                }
                else
                {
                    ShowMessageBoxA("Can't open the File.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        public override void OnModuleMemo()
        {
            base.OnModuleMemo();

            try
            {
                // FG_MEMO, NO_SLIP_REL
                string _noSlipRel = A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE"]);
                if (_noSlipRel != string.Empty)
                {
                    object[] obj = {
                        "INV",
                        _noSlipRel
                    };
                    POPUP_MEMO pop = new POPUP_MEMO(obj);
                    pop.ShowDialog();
                }
                else
                {
                    ShowMessageBoxA("Can't open the Memo.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        public override void OnHelpMenual()
        {
            try
            {
                base.OnHelpMenual();
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        public override void OnMovePartner()
        {
            try
            {
                base.OnMovePartner();
                MdiForm.CreateChildForm("MAS.M_MAS_PARTNER");
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        public override void OnPrint()
        {
            try
            {
                DataRow row = _HEADER.CurrentRow;
                string cdBiz = A.GetString(_HEADER.CurrentRow["CD_BIZ"]);
                string fgInvoice = A.GetString(_HEADER.CurrentRow["FG_INVOICE"]);

                string[] ReportFile = new string[1];
                ReportFile[0] = "PAYMENT_REQUEST";

                RptHelper.ReportView(ReportFile,
                            new string[] { "P_CD_FIRM", "P_CD_BIZ", "P_CD_USER", "P_NO_SLIP_INVOICE" },
                            new object[] { Global.FirmCode, cdBiz, Global.UserID, row["NO_SLIP_INVOICE"] });

                _D.UpdateInvoicePrint(cdBiz, A.GetString(row["NO_SLIP_INVOICE"]));
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        #endregion

        #region ▶ button Event ------

        private void AButton_Save_Click(object sender, EventArgs e)
        {
            OnSave();
        }

        private void AButton_Reflact_Click(object sender, EventArgs e)
        {
            // 조회 된 내용이 없으면 Reflact 하지 않음
            if (aTextEdit_ShipmentNo.Text == string.Empty) return;
            //if (Global.BizCode == "TYO")
            //{
            //    if (aTextEdit_ShipmentNo.Text == string.Empty) return;
            //}
            //else
            //{
            //    if (aTextEdit_MBLNo.Text == string.Empty) return;
            //}

            // Valid Check
            if (aTextEdit_VendorInvNo.Text == string.Empty) return;
            if (aNumericText_TotalAmount.Text == "0.00") return;

            aTextEdit_InvoiceNo.Text = aTextEdit_VendorInvNo.Text;            
            aDateEdit_TM_INVOICE_RECEIVED.Text = A.GetToday;

            //2020.10.16 HYJ EU AP 관리 개선 -> INV DATE, DUE DATE 입력 제거 
            if (Global.BizCode == "TYO" || Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
            {
                aDateEdit_DueDate.Text = aDateEdit_VendorDueDate.Text;
                aDateEdit_InvoiceDate.Text = aDateEdit_VendorReceivedDate.Text;
            }

            if (Global.BizCode == "TYO") return;

            //아래는 유럽,미국 전용
            Button_ADD_Click(null, null);

            string oceanFreight = string.Empty;
            if (Global.BizCode == "MIL")
                oceanFreight = "OE";
            else 
                oceanFreight = "OF";

            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["CD_FREIGHT"], oceanFreight);
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["NM_FREIGHT"], "OCEAN FREIGHT");
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["FG_CALC"], "BL");
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["QT_UNIT"], 1);
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], aNumericText_TotalAmount.Text);

            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_FREIGHT_VAT"], 0);
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], aNumericText_TotalAmount.Text);
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], 0);
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], aNumericText_TotalAmount.Text);

            bandedGridView1.UpdateCurrentRow();

            DataValidCheck();
        }

        private void AButton_OrderPopUp_Click(object sender, EventArgs e)
        {
            try
            {
                if (Global.BizCode == "TYO")
                {
                    POPUP_BL_SEARCH pop = new POPUP_BL_SEARCH(Global.BizCode, "M", "S", "O");

                    if (pop.ShowDialog() == DialogResult.OK)
                    {
                        DataRow row = (DataRow)pop.ReturnData["ReturnData"];
                        string cdBiz = A.GetString(row["CD_BIZ"]);
                        string noSlipBl = A.GetString(row["NO_SLIP_BL"]);

                        DoSearch(cdBiz, noSlipBl);
                    }
                }
                else
                {
                    POPUP_PROGRESS_SEARCH pop = new POPUP_PROGRESS_SEARCH("S", "O");

                    if (pop.ShowDialog() == DialogResult.OK)
                    {
                        DataRow row = (DataRow)pop.ReturnData["ReturnData"];
                        string cdBiz = A.GetString(row["CD_BIZ"]);
                        string noSlipOrder = A.GetString(row["NO_SLIP_PROGRESS"]);

                        DoSearch(cdBiz, noSlipOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void Button_ADD_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender == null)
                {
                    if (bandedGridView1.RowCount == 0)
                    {
                        bandedGridView1.AddNewRow();
                        bandedGridView1.UpdateCurrentRow();
                    }
                }
                else
                {
                    bandedGridView1.AddNewRow();
                    bandedGridView1.UpdateCurrentRow();
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void Button_DEL_Click(object sender, EventArgs e)
        {
            try
            {
                if (bandedGridView1.GetFocusedDataRow().RowState != DataRowState.Added)
                {
                    string invoiceStatus = A.GetString(bandedGridView1.GetFocusedRowCellValue("CD_INVOICE_STATUS"));

                    if (invoiceStatus == "CL")
                    {
                        return;
                    }
                }

                bandedGridView1.DeleteSelectedRows();
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void AButton_AP_Approval_Click(object sender, EventArgs e)
        {
            if (_noSlipInvoice == string.Empty)
            {
                ShowMessageBoxA("INVOICE has not been saved.", MessageType.Information);
                return;
            }

            DataTable dtInvoiceChanges = _HEADER.GetChanges();
            DataTable dtFreightChanges = aGrid_Freight.GetChanges();

            if (dtInvoiceChanges == null && dtFreightChanges == null)
            {
                ShowMessageBoxA("Changed contents exist. Please try again after saving the data.", MessageType.Information);
                return;
            }

            if (ShowMessageBoxA("Do you really want to request this?", MessageType.Question) == DialogResult.Yes)
            {
                OnSavePaymentRequest();

                _HEADER.AcceptChanges();
                aGrid_Freight.AcceptChanges();
            }
        }

        private void Button_ADD_GotFocus(object sender, EventArgs e)
        {
            try
            {
                bandedGridView1.Focus();
                bandedGridView1.FocusedColumn = bandedGridView1.Columns["CD_FREIGHT"];
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void AButton_Detect_Click(object sender, EventArgs e)
        {
            InitDocument();
            InitKeywords();

            int largestEdgeLength = 1000;
            int page = pdfViewer1.PageCount; //CurrentPageNumber;

            if (_pdfFilePath == string.Empty) return;

            // Create a PDF Document Processor.
            using (PdfDocumentProcessor processor = new PdfDocumentProcessor())
            {
                processor.LoadDocument(_pdfFilePath);

                string text = processor.GetPageText(1);
                string classifiedDocument = ClassifyDocument(text);

                if (text.Contains("Hyundai Merchant Marine"))
                {
                    classifiedDocument = "hmm_vendor_invoice";
                }
                else if (text.Contains("HASLJ") && Global.BizCode == "TYO")
                {
                    classifiedDocument = "heung_vendor_invoice";
                }
                else if (text.Contains("SNKO") && Global.BizCode == "TYO")
                {
                    classifiedDocument = "heung_vendor_invoice";
                }
                else if (text.Contains("PCSL") && Global.BizCode == "TYO")
                {
                    classifiedDocument = "pegasus_vendor_invoice";
                }
                else if (text.Contains("NSSL") && Global.BizCode == "TYO")
                {
                    classifiedDocument = "namsung_vendor_invoice";
                }
                Console.WriteLine(classifiedDocument);

                for (int i = 1; i <= page; i++)
                {
                    string pageText = processor.GetPageText(i);

                    if (pageText == string.Empty)
                    {
                        // Export pages to bitmaps.
                        Bitmap image = processor.CreateBitmap(i, largestEdgeLength);

                        // Save the bitmaps.
                        string filename = Application.StartupPath + @"/Temp/" + Path.GetFileNameWithoutExtension(_pdfFilePath) + "-" + i + ".jpg";

                        FileInfo fi = new FileInfo(filename);
                        if (!fi.Exists)
                        {
                            image.Save(filename);
                        }

                        //사용자 지정 영역이 해당 페이지에 있다면 그 안의 내용만 OCR 처리
                        string cropFilename = Application.StartupPath + @"/Temp/" + Path.GetFileNameWithoutExtension(_pdfFilePath) + "-" + i + "-crop-" + DateTime.Now.ToString("yyyyMMddHHmmsss") + ".jpg";
                        if (pdfViewer1.GetSelectionContent().Image != null)
                        {
                            pdfViewer1.GetSelectionContent().Image.Save(cropFilename);
                        }

                        if (pdfViewer1.GetSelectionContent().Image != null)
                        {
                            //pictureBox1.Load(cropFilename);
                            DetectText(cropFilename);
                        }
                        else
                        {
                            //pictureBox1.Load(filename);
                            DetectText(filename);
                        }
                    }
                    else
                    {
                        if (classifiedDocument == "default")
                        {
                            //구분이 안되면 추출한 텍스트를 다 보여준다.
                            //richTextBox1.Text += temp;
                        }
                        else
                        {
                            if (Global.BizCode == "TYO")
                            {
                                GetKeyFeatures_JP(classifiedDocument, pageText, i);
                            }
                            else if (Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
                            {
                                GetKeyFeatures_US(classifiedDocument, pageText, i);
                            }
                            else
                            {
                                GetKeyFeatures_EU(classifiedDocument, pageText, i);
                            }

                            DataValidCheck();

                            if (aTextEdit_VendorRefNo.Text != string.Empty)
                            {
                                DoSearch2(Global.BizCode, aTextEdit_VendorRefNo.Text);

                                if (aTextEdit_VendorInvNo.Text != string.Empty
                                    //&& aDateEdit_VendorDueDate.Text != string.Empty
                                    //&& aDateEdit_VendorReceivedDate.Text != string.Empty
                                    && aNumericText_TotalAmount.Text != "0.00")
                                {
                                    AButton_Reflact_Click(null, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AButton_Search_Click(object sender, EventArgs e)
        {
            if (aTextEdit_VendorRefNo.Text == string.Empty) return;

            // MB/L No.로 조회하는 기능
            DoSearch2(Global.BizCode, aTextEdit_VendorRefNo.Text);
        }

        /* 사용하지 않음 
        private void AButton_Container_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dtContainer = GetdtContainer();

                if (dtContainer == null || dtContainer.Rows.Count == 0) return;

                DataTable dt = dtContainer.DefaultView.ToTable();
                DataTable dtGroupBy = dt.DefaultView.ToTable(true, new string[] { "TP_CONTAINER" });

                foreach (DataRow row in dtGroupBy.Rows)
                {
                    int qty = dt.Select("TP_CONTAINER = '" + A.GetString(row["TP_CONTAINER"]) + "'").Length;
                    Button_ADD_Click(null, null);
                    DataRow newRow = bandedGridView1.GetDataRow(bandedGridView1.FocusedRowHandle);

                    if (Global.BizCode == "TYO" || Global.BizCode == "OSA")
                    {
                        newRow["CD_FREIGHT"] = "OF-OE";     //OCEAN FREIGHT로 FIX
                        newRow["NM_FREIGHT"] = "OCEAN FREIGHT" + "(" + A.GetString(row["TP_CONTAINER"]) + ")";
                    }
                    else if (Global.BizCode == "PAR")
                    {
                        newRow["CD_FREIGHT"] = "OF";
                        newRow["NM_FREIGHT"] = "O/F CHARGES / FRET MARITIME" + "(" + A.GetString(row["TP_CONTAINER"]) + ")";
                    }
                    else if (Global.BizCode == "FRA" || Global.BizCode == "HAM")
                    {
                        newRow["CD_FREIGHT"] = "OF";
                        newRow["NM_FREIGHT"] = "OCEAN FREIGHT" + "(" + A.GetString(row["TP_CONTAINER"]) + ")";
                    }
                    else if (Global.BizCode == "MIL")
                    {
                        newRow["CD_FREIGHT"] = "OE";
                        newRow["NM_FREIGHT"] = "OCEAN FREIGHT" + "(" + A.GetString(row["TP_CONTAINER"]) + ")";
                    }
                    else if (Global.BizCode == "LAX" || Global.BizCode == "NYC" || Global.BizCode == "CHI")
                    {
                        newRow["CD_FREIGHT"] = "OF";
                        newRow["NM_FREIGHT"] = "OCEAN FREIGHT" + "(" + A.GetString(row["TP_CONTAINER"]) + ")";
                    }

                    newRow["FG_CALC"] = "CNTR";
                    newRow["QT_UNIT"] = qty;
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }
        private void AButton_ARCreate_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }
        private void AButton_Copy_Click(object sender, EventArgs e)
        {
            try
            {
                POPUP_INVOICE_SEARCH pop = new POPUP_INVOICE_SEARCH("M_INT_INVOICE_AP");

                if (pop.ShowDialog() != DialogResult.OK) return;

                DataTable returnDt = (DataTable)pop.ReturnData["ReturnDataTable"];

                foreach (DataRow row in returnDt.Rows)
                {
                    Button_ADD_Click(null, null);
                    DataRow newRow = bandedGridView1.GetDataRow(bandedGridView1.FocusedRowHandle);

                    newRow["TYPE"] = row["TYPE"];
                    newRow["CD_FREIGHT"] = row["CD_FREIGHT"];
                    newRow["NM_FREIGHT"] = row["NM_FREIGHT"];
                    newRow["FG_CALC"] = row["FG_CALC"];

                    //단위별 값 매칭
                    //if (A.GetString(row["FG_CALC"]) == "CWGT")
                    //    newRow["QT_UNIT"] = aNumericText_CWGTK.DecimalValue;
                    //else if (A.GetString(row["FG_CALC"]) == "GWGT")
                    //    newRow["QT_UNIT"] = aNumericText_GWGTK.DecimalValue;
                    //else if (A.GetString(row["FG_CALC"]) == "PCS")
                    //    newRow["QT_UNIT"] = aNumericText_PKG.DecimalValue;
                    //else if (A.GetString(row["FG_CALC"]) == "CINV")
                    //    newRow["QT_UNIT"] = 1m;
                    //else if (A.GetString(row["FG_CALC"]) == "CBM")
                    //    newRow["QT_UNIT"] = aNumericText_CBM.DecimalValue < 1m ? 1m : aNumericText_CBM.DecimalValue;
                    //else if (A.GetString(row["FG_CALC"]) == "RT")
                    //    newRow["QT_UNIT"] = CalcRevenueTon();
                    //else if (A.GetString(row["FG_CALC"]) == "CNTR")
                    //{
                    //    DataTable dtContainer = GetdtContainer();
                    //    newRow["QT_UNIT"] = dtContainer.Rows.Count == 0 ? 1m : A.GetDecimal(dtContainer.Rows.Count);
                    //}
                    //else if (A.GetString(row["FG_CALC"]) == "TON")
                    //{
                    //    newRow["QT_UNIT"] = decimal.Round(aNumericText_GWGTK.DecimalValue / 1000m, 2, MidpointRounding.AwayFromZero);
                    //    newRow["QT_UNIT"] = A.GetDecimal(newRow["QT_UNIT"]) < 1m ? 1m : A.GetDecimal(newRow["QT_UNIT"]);
                    //}
                    //else
                        newRow["QT_UNIT"] = 1m;

                    newRow["RT_UNIT"] = row["RT_UNIT"];
                    newRow["RT_FREIGHT_VAT"] = row["RT_FREIGHT_VAT"];
                    newRow["CD_CURRENCY"] = row["CD_CURRENCY"];
                    newRow["RT_XCRT"] = row["RT_XCRT"];

                    int floatPoint = 0;

                    if (A.GetString(newRow["CD_CURRENCY"]) != "KRW" && A.GetString(newRow["CD_CURRENCY"]) != "JPY")
                    {
                        floatPoint = 2;
                    }

                    newRow["AM_FREIGHT_COST"] = decimal.Round(A.GetDecimal(newRow["QT_UNIT"]) * A.GetDecimal(newRow["RT_UNIT"]) * A.GetDecimal(newRow["RT_XCRT"]), floatPoint, MidpointRounding.AwayFromZero);
                    newRow["AM_FREIGHT_VAT_COST"] = decimal.Round(A.GetDecimal(newRow["AM_FREIGHT_COST"]) * A.GetDecimal(newRow["RT_FREIGHT_VAT"]) * 100m, floatPoint, MidpointRounding.AwayFromZero);
                    newRow["AM_FREIGHT_SUM_COST"] = A.GetDecimal(newRow["AM_FREIGHT_COST"]) + A.GetDecimal(newRow["AM_FREIGHT_VAT_COST"]);
                }

                bandedGridView1.UpdateCurrentRow();
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }
        private void AButton_APPROVAL_Click(object sender, EventArgs e)
        {
            try
            {
                //if (aLabel_PDF.Text == string.Empty)
                //{
                //    ShowMessageBoxA("If you do not attach file, you can not approve it.", MessageType.Warning);
                //    return;
                //}

                //if (gridView1.DataRowCount == 0)
                //{
                //    ShowMessageBoxA("There are no A/P to Payment Request.", MessageType.Warning);
                //    return;
                //}

                //string noSlipInvoiceAp = A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE_AP"]);
                //if (noSlipInvoiceAp != string.Empty)
                //{
                //    ShowMessageBoxA("It is already a Payment Request. Please process in Payment Request Search menu.", MessageType.Information);
                //    return;
                //}

                DataTable dtInvoiceChanges = _HEADER.GetChanges();
                DataTable dtFreightChanges = aGrid_Freight.GetChanges();
                if (dtInvoiceChanges != null || dtFreightChanges != null)
                {
                    ShowMessageBoxA("Changed contents exist. Please try again after saving the data.", MessageType.Information);
                    return;
                }

                if (ShowMessageBoxA("Do you really want to request this?", MessageType.Question) == DialogResult.Yes)
                {
                    object[] obj = new object[] {
                        A.GetString(_HEADER.CurrentRow["CD_BIZ"]),
                        A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE_AP"]),
                        _noSlipInvoice,
                        aCodeText_Vendor.CodeValue,
                        aCodeText_Vendor.CodeName,
                        A.GetString(_HEADER.CurrentRow["CD_CURRENCY"])
                    };
                    MdiForm.CreateChildForm("INT.M_INT_INVOICE_COMBINE", obj);
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }
        private void AButton_Tariff_Click(object sender, EventArgs e)
        {
            try
            {
                TariffCalculate();
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        */

        #endregion

        #region ▶ Grid Event --------

        private void GridView1_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            try
            {
                GridView view = sender as GridView;
                GridControl control = view.GridControl;

                DataTable dt = control.DataSource as DataTable;
                decimal maxSeq = A.GetDecimal(dt.Compute("MAX(SEQ_FREIGHT)", ""));
                string cdCurrency = string.Empty;

                DataRow rowOffice = MasterHelper.GetOffice(Global.BizCode);
                if (rowOffice != null) cdCurrency = A.GetString(rowOffice["CD_CURRENCY"]);

                view.SetRowCellValue(e.RowHandle, view.Columns["SEQ_FREIGHT"], ++maxSeq);
                view.SetRowCellValue(e.RowHandle, view.Columns["CD_CURRENCY"], A.GetString(aLookUpEdit_Currency.EditValue) == "" ? cdCurrency : aLookUpEdit_Currency.EditValue);
                view.SetRowCellValue(e.RowHandle, view.Columns["RT_XCRT"], 1m);
                //view.SetRowCellValue(e.RowHandle, view.Columns["TM_INVOICE_POST"], _HEADER.CurrentRow["TM_INVOICE_POST"]);
                string tmClose = A.GetString(_HEADER.CurrentRow["TM_CLOSE"]);

                if (tmClose == string.Empty)
                {
                    view.SetRowCellValue(e.RowHandle, view.Columns["TM_INVOICE_POST"], _HEADER.CurrentRow["TM_INVOICE_POST"]);
                }
                else
                {
                    DateTime date = new DateTime(Int32.Parse(tmClose.Substring(0, 4)), Int32.Parse(tmClose.Substring(4, 2)), 1);
                    view.SetRowCellValue(e.RowHandle, view.Columns["TM_INVOICE_POST"], _HEADER.CurrentRow["TM_INVOICE_POST"]);
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void BandedGridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            try
            {
                /*
                 * 2016.10.24 - KJH
                 * 계산 로직 반영
                 */

                if (bandedGridView1.FocusedColumn.FieldName != e.Column.FieldName) return;

                decimal rt = 100;
                decimal _qt_ = A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"]));
                decimal _rt_ = A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["RT_UNIT"]));
                decimal _vat_ = A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["RT_FREIGHT_VAT"])) / rt;
                decimal _xcrt_ = A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["RT_XCRT"]));

                string cdCurrency = A.GetString(aLookUpEdit_Currency.EditValue);

                switch (e.Column.FieldName)
                {
                    case "CD_CURRENCY":
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], _qt_ * _rt_ * _xcrt_);
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], _qt_ * _rt_ * _vat_ * _xcrt_);
                        break;
                    case "FG_CALC":
                        decimal _qtUnit = 1m;
                        //decimal _qtGrsWgt = A.GetDecimal(aNumericText_GWGTK.Text);
                        //decimal _qtChgWgt = A.GetDecimal(aNumericText_CWGTK.Text);
                        //decimal _qtMeas = A.GetDecimal(aNumericText_CBM.Text);
                        //decimal _qtPackage = A.GetDecimal(aNumericText_PKG.Text);
                        //conatiner 수량
                        decimal _qtContainer = 1;

                        switch (e.Value.ToString())
                        {
                            //BL,CBM,CNTR,CWGT,GWGT,PCS,RT
                            case "BL":
                                bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"], _qtUnit);
                                break;
                            case "CBM":
                                //_qtUnit = _qtMeas < 1m ? 1m : _qtMeas;
                                //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"], _qtUnit);
                                break;
                            case "CNTR":
                                _qtUnit = _qtContainer;
                                bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"], _qtUnit);
                                break;
                            case "CWGT":
                                //_qtUnit = _qtChgWgt;
                                //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"], _qtUnit);
                                break;
                            case "GWGT":
                                //_qtUnit = _qtGrsWgt;
                                //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"], _qtUnit);
                                break;
                            case "PCS":
                                //_qtUnit = _qtPackage;
                                //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"], _qtUnit);
                                break;
                            case "RT":
                                //_qtUnit = decimal.Round((_qtGrsWgt / 1000), 3, MidpointRounding.AwayFromZero) > _qtMeas ? decimal.Round((_qtGrsWgt / 1000), 3, MidpointRounding.AwayFromZero) : _qtMeas;
                                //_qtUnit = _qtUnit < 1m ? 1m : _qtUnit;
                                //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"], _qtUnit);
                                break;
                            case "TON":
                                //_qtUnit = decimal.Round(_qtGrsWgt / 1000m, 2, MidpointRounding.AwayFromZero);
                                //_qtUnit = _qtUnit < 1m ? 1m : _qtUnit;
                                //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["QT_UNIT"], _qtUnit);
                                break;
                            default:
                                break;
                        }

                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], _xcrt_ * _rt_ * _qtUnit);
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], _xcrt_ * _rt_ * _qtUnit * _vat_);

                        break;
                    case "QT_UNIT":
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], _xcrt_ * _rt_ * A.GetDecimal(e.Value));
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], _xcrt_ * _rt_ * A.GetDecimal(e.Value) * _vat_);
                        //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"])) + A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"])));
                        break;
                    case "RT_UNIT":
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], _xcrt_ * _qt_ * A.GetDecimal(e.Value));
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], _xcrt_ * _qt_ * A.GetDecimal(e.Value) * _vat_);
                        //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"])) + A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"])));
                        break;
                    case "RT_FREIGHT_VAT":
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], _xcrt_ * _qt_ * _rt_ * A.GetDecimal(e.Value) / rt);
                        //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"])) + A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"])));
                        break;
                    case "RT_XCRT":
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], _qt_ * _rt_ * A.GetDecimal(e.Value));
                        bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], _qt_ * _rt_ * _vat_ * A.GetDecimal(e.Value));
                        //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"])) + A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"])));
                        break;
                    case "AM_FREIGHT_COST":
                        //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], A.GetDecimal(e.Value) + A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"])));
                        break;
                    case "AM_FREIGHT_VAT_COST":
                        //bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], A.GetDecimal(e.Value) + A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"])));
                        break;
                    default:
                        break;
                }

                int floatPoint = 0;
                if (cdCurrency != "KRW" && cdCurrency != "JPY")
                {
                    floatPoint = 2;
                }

                bandedGridView1.CellValueChanged -= BandedGridView1_CellValueChanged;

                bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], decimal.Round(A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"])), floatPoint, MidpointRounding.AwayFromZero));
                bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], decimal.Round(A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"])), floatPoint, MidpointRounding.AwayFromZero));
                bandedGridView1.SetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"])) + A.GetDecimal(bandedGridView1.GetRowCellValue(e.RowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"])));

                bandedGridView1.CellValueChanged += BandedGridView1_CellValueChanged;
                bandedGridView1.UpdateCurrentRow();
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void BandedGridView1_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (bandedGridView1.GetFocusedDataRow().RowState != DataRowState.Added)
                {
                    string invoiceStatus = A.GetString(bandedGridView1.GetFocusedRowCellValue("CD_INVOICE_STATUS"));

                    if (invoiceStatus == "CL")
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void RepositoryItemLookUpEdit3_Closed(object sender, DevExpress.XtraEditors.Controls.ClosedEventArgs e)
        {
            bandedGridView1.CloseEditor();
            bandedGridView1.UpdateCurrentRow();
        }

        private void RepositoryItemLookUpEdit5_QueryCloseUp(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                DevExpress.Utils.Win.IPopupControl popupEdit = sender as DevExpress.Utils.Win.IPopupControl;
                DevExpress.XtraEditors.Popup.PopupLookUpEditForm popupWindow = popupEdit.PopupWindow as DevExpress.XtraEditors.Popup.PopupLookUpEditForm;
                DataRowView row = popupWindow.Properties.GetDataSourceRowByKeyValue(popupWindow.ResultValue) as DataRowView;

                if (row != null)
                {
                    bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["CD_FREIGHT"], row["CODE"]);
                    bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["NM_FREIGHT"], row["NAME"]);
                    bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_FREIGHT_VAT"], row["NAME1"]);

                    bandedGridView1.FocusedColumn = bandedGridView1.Columns["FG_CALC"];
                    bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["FG_CALC"], row["NAME2"]);
                    bandedGridView1.FocusedColumn = bandedGridView1.Columns["NM_FREIGHT"];

                    bandedGridView1.UpdateCurrentRow();
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        #endregion

        #region ▶ Control Event -----

        private void ADateEdit_TM_INVOICE_RECEIVED_EditValueChanged(object sender, EventArgs e)
        {
            DataValidCheck();
        }

        private void ADateEdit_DueDate_EditValueChanged(object sender, EventArgs e)
        {
            DataValidCheck();
        }

        private void ATextEdit_InvoiceNo_EditValueChanged(object sender, EventArgs e)
        {
            DataValidCheck();
        }

        private void ADateEdit_InvoiceDate_EditValueChanged(object sender, EventArgs e)
        {
            if (!aDateEdit_InvoiceDate.IsEditorActive) return;
            SettingDueDate();
        }

        private void ACodeText_Vendor_AfterCodeValueChanged(object sender, NF.Framework.Adv.Controls.aControlHelper.aControlEventArgs e)
        {
            try
            {
                NF.Framework.Adv.Controls.aCodeText aCodeText = sender as NF.Framework.Adv.Controls.aCodeText;
                DataRow row = e.ReturnDataRow;

                //if (Global.BizCode == "FRA" || Global.BizCode == "HAM" || Global.BizCode == "PAR" || Global.BizCode == "MIL")
                //{
                //    string tpPartner = A.GetString(row["TP_PARTNER"]);

                //    if (tpPartner != "AC" && tpPartner != "CB" && tpPartner != "FR" && tpPartner != "LN" && tpPartner != "RC" && tpPartner != "TK")
                //    {
                //        //e.Cancel = true;
                //        aCodeText_Vendor.SetCodeNameNValue(string.Empty, string.Empty);
                //        return;
                //    }
                //}

                SettingDueDate();

                aMemoEdit_PaidToAddr.Text = A.GetString(row["DC_ADDRESS_ACCT"]);

                DataTable dt = _D.SearchAttn(new object[] { Global.FirmCode, e.CodeValue });
                if (dt.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    //aTextEdit_PaidToAttn.Text = A.GetString(dt.Rows[0]["NM"]);
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void aLookUpEdit_Currency_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (bandedGridView1.RowCount == 0) return;

                bandedGridView1.FocusedColumn = bandedGridView1.Columns["CD_CURRENCY"];

                for (int i = bandedGridView1.RowCount - 1; i >= 0; i--)
                {
                    bandedGridView1.SetRowCellValue(i, bandedGridView1.Columns["CD_CURRENCY"], A.GetString(aLookUpEdit_Currency.EditValue));
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void ATextEdit_InvoiceNo_LostFocus(object sender, EventArgs e)
        {
            try
            {
                string cdBiz = A.GetString(_HEADER.CurrentRow["CD_BIZ"]) == string.Empty ? Global.BizCode : A.GetString(_HEADER.CurrentRow["CD_BIZ"]);
                DataTable dtCheckInvoiceNo = _D.CheckInvoiceNo(cdBiz, A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE"]), aTextEdit_InvoiceNo.Text);

                if (dtCheckInvoiceNo.Rows.Count > 0)
                    aLabel_Duplicate.Visible = true;
                else
                    aLabel_Duplicate.Visible = false;
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void ATextEdit_ShipmentNo_DoubleClick(object sender, EventArgs e)
        {
            if (Global.BizCode == "TYO")
            {
                if (aTextEdit_ShipmentNo.Text != string.Empty)
                {
                    object[] obj = new object[] { "S", A.GetString(_HEADER.CurrentRow["CD_BIZ"]), A.GetString(_HEADER.CurrentRow["NO_INVOICE_REL"]) };
                    MdiForm.CreateChildForm("INT.M_INT_OE_MBL", obj, A.GetString(_HEADER.CurrentRow["NO_PROGRESS"]));
                }
            }
            else
            {
                if (aTextEdit_ShipmentNo.Text != string.Empty)
                {
                    if (A.GetString(aLookUpEdit_ShipMode.EditValue) == "CONSOL")
                    {
                        object[] obj = new object[] { A.GetString(_HEADER.CurrentRow["CD_BIZ"]), A.GetString(_HEADER.CurrentRow["NO_SLIP_SCHEDULE"]) };
                        MdiForm.CreateChildForm("INT.M_INT_OE_CONSOL_MASTER", obj, A.GetString(_HEADER.CurrentRow["NO_SLIP_SCHEDULE"]));
                    }
                    else
                    {
                        if (Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
                        {
                            object[] obj = new object[] { "S", A.GetString(_HEADER.CurrentRow["CD_BIZ"]), A.GetString(_HEADER.CurrentRow["NO_SLIP_PROGRESS"]) };
                            MdiForm.CreateChildForm("INT.M_INT_OE_SHIPMENT_PORTAL_US", obj);
                        }
                        else
                        {
                            object[] obj = new object[] { "S", A.GetString(_HEADER.CurrentRow["CD_BIZ"]), A.GetString(_HEADER.CurrentRow["NO_SLIP_PROGRESS"]) };
                            MdiForm.CreateChildForm("INT.M_INT_OE_SHIPMENT_PORTAL", obj);
                        }
                    }
                }
            }
        }

        private void ATextEdit_RequestNo_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (aTextEdit_RequestNo.Text != string.Empty)
                {
                    object[] obj = null;

                    if (A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE_AP"]) != "")
                    {
                        obj = new object[] {
                                                A.GetString(_HEADER.CurrentRow["CD_BIZ"]),
                                                A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE_AP"]),
                                                A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE"]),
                                                aCodeText_Vendor.CodeValue,
                                                aCodeText_Vendor.CodeName,
                                                A.GetString(_HEADER.CurrentRow["CD_CURRENCY"])
                                            };
                    }
                    //else
                    //{

                    //    string noInvoiceRel = _HEADER.CurrentRow["NO_INVOICE_REL"].ToString();
                    //    string cdPartnerCarrier = _HEADER.CurrentRow["CD_PARTNER_CARRIER"].ToString();

                    //    string noInvoiceAp = SettingNoInvoiceAp(noInvoiceRel, cdPartnerCarrier);

                    //    if (noInvoiceAp != "")
                    //    {
                    //        SettingAPRequest(cdBiz, noInvoiceAp);
                    //    }
                    //}

                    MdiForm.CreateChildForm("INT.M_INT_INVOICE_COMBINE", obj);
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void ATextEdit_AttachInvFile_DoubleClick(object sender, EventArgs e)
        {
            if (aTextEdit_AttachInvFile.Text == "") return;

            object[] obj = new object[] { Global.FirmCode, _noSlipInvoice };
            DataTable dtFile = _D.SearchFile(obj);

            if (dtFile.Rows.Count > 0)
            {
                string nmFile = A.GetString(dtFile.Rows[0]["NM_FILE"]);
                string nmFilePath = A.GetString(dtFile.Rows[0]["NM_FILE_PATH"]);
                string downloadPath = Application.StartupPath + @"/TempDownload";
                DirectoryInfo di = new DirectoryInfo(downloadPath);

                if (di.Exists == false)
                {
                    di.Create();
                }

                bool result = _ftpUtil.Download(downloadPath + "/" + nmFile, nmFilePath + "/" + nmFile);

                if (result)
                {
                    System.Diagnostics.Process.Start(downloadPath + "/" + nmFile);
                }
            }
        }

        private void PdfViewer1_DragEnter(object sender, DragEventArgs e)
        {
            // for this program, we allow a file to be dropped from Explorer
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            //    or this tells us if it is an Outlook attachment drop
            else if (e.Data.GetDataPresent("FileGroupDescriptor"))
            {
                e.Effect = DragDropEffects.Copy;
            }
            //    or none of the above
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PdfViewer1_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = null;

            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                {
                    fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                    // handle each file passed as needed
                    foreach (string fileName in fileNames)
                    {
                        string ext = Path.GetExtension(fileName);
                        if (ext.ToUpper() == ".PDF")
                        {
                            SettingPDF(fileName);
                        }
                        // do what you are going to do with each filename
                    }
                }
                else if (e.Data.GetDataPresent("FileGroupDescriptor"))
                {
                    //
                    // the first step here is to get the filename
                    // of the attachment and
                    // build a full-path name so we can store it
                    // in the temporary folder
                    //

                    // set up to obtain the FileGroupDescriptor
                    // and extract the file name
                    Stream theStream = (Stream)e.Data.GetData("FileGroupDescriptor");
                    byte[] fileGroupDescriptor = new byte[512];
                    theStream.Read(fileGroupDescriptor, 0, 512);
                    // used to build the filename from the FileGroupDescriptor block
                    StringBuilder fileName = new StringBuilder("");
                    // this trick gets the filename of the passed attached file
                    for (int i = 76; fileGroupDescriptor[i] != 0; i++)
                    {
                        fileName.Append(Convert.ToChar(fileGroupDescriptor[i]));
                    }
                    theStream.Close();
                    string path = Path.GetTempPath();
                    // put the zip file into the temp directory
                    string theFile = path + fileName.ToString();
                    // create the full-path name

                    //
                    // Second step:  we have the file name.
                    // Now we need to get the actual raw
                    // data for the attached file and copy it to disk so we work on it.
                    //

                    // get the actual raw file into memory
                    MemoryStream ms = (MemoryStream)e.Data.GetData(
                        "FileContents", true);
                    // allocate enough bytes to hold the raw data
                    byte[] fileBytes = new byte[ms.Length];
                    // set starting position at first byte and read in the raw data
                    ms.Position = 0;
                    ms.Read(fileBytes, 0, (int)ms.Length);
                    // create a file and save the raw zip file to it
                    FileStream fs = new FileStream(theFile, FileMode.Create);
                    fs.Write(fileBytes, 0, (int)fileBytes.Length);

                    fs.Close();  // close the file

                    FileInfo tempFile = new FileInfo(theFile);

                    // always good to make sure we actually created the file
                    if (tempFile.Exists == true)
                    {
                        // for now, just delete what we created
                        //tempFile.Delete();
                        string ext = Path.GetExtension(theFile);
                        if (ext.ToUpper() == ".PDF")
                        {
                            SettingPDF(theFile);
                        }
                    }
                    else
                    {
                        Console.WriteLine("File was not created!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DragDrop function: " + ex.Message);

                // don't use MessageBox here - Outlook or Explorer is waiting !
            }
        }

        private void PdfViewer2_DocumentChanged(object sender, DevExpress.XtraPdfViewer.PdfDocumentChangedEventArgs e)
        {
            _pdfFilePath = ((DevExpress.XtraPdfViewer.PdfViewer)sender).DocumentFilePath;
            aTextEdit_filename.Text = _pdfFilePath;
        }

        #endregion

        #region ▶ Method ------------

        #region PDF

        public void SettingPDF(string filename)
        {
            _noSlipInvoice = string.Empty;
            _pdfFilePath = string.Empty;

            OnInsert();

            pdfViewer1.LoadDocument(filename);

            aLabel_FileNo.Visible = false;
            aLabel_FileYes.Visible = true;

            AButton_Detect_Click(null, null);
        }

        /// <summary>
        /// 학습한 서류 목록
        /// </summary>
        public void InitDocument()
        {
            _documentFrequency = new Dictionary<string, double>();


            #region - vendor invoice

            if (Global.BizCode == "TYO")
            {
                //JP
                _documentFrequency.Add("kmtc_vendor_invoice", 0.0);
                _documentFrequency.Add("sitc_vendor_invoice", 0.0);
                _documentFrequency.Add("heung_vendor_invoice", 0.0);
                _documentFrequency.Add("sinokor_vendor_invoice", 0.0);
                _documentFrequency.Add("namsung_vendor_invoice", 0.0);
                _documentFrequency.Add("pegasus_vendor_invoice", 0.0);
            }
            else if (Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
            {
                //EU
                _documentFrequency.Add("one_vendor_invoice", 0.0);
                _documentFrequency.Add("hmm_vendor_invoice", 0.0);
            }
            else
            {
                //EU
                _documentFrequency.Add("one_vendor_invoice", 0.0);
                _documentFrequency.Add("hapag_vendor_invoice", 0.0);
                _documentFrequency.Add("hmm_vendor_invoice", 0.0);
                _documentFrequency.Add("maersk_vendor_invoice", 0.0);
                _documentFrequency.Add("cosco_vendor_invoice", 0.0);
            }
            _documentFrequency.Add("default", 0.0);
            //_documentFrequency.Add("yangming_vendor_invoice", 0.0);
            //_documentFrequency.Add("evergreen_vendor_invoice", 0.0);
            //_documentFrequency.Add("cosco_vendor_invoice", 0.0);

            #endregion
        }

        public void InitKeywords()
        {
            _keywords = new Dictionary<string, string[]>();


            #region - vendor invoice

            if (Global.BizCode == "TYO")
            {
                //JP
                _keywords.Add("kmtc_vendor_invoice", new string[] { "KMTC" });
                _keywords.Add("sitc_vendor_invoice", new string[] { "SITC" });
                _keywords.Add("heung_vendor_invoice", new string[] { "HASLJ" });
                _keywords.Add("sinokor_vendor_invoice", new string[] { "Sinokor", "SNKO" });
                _keywords.Add("namsung_vendor_invoice", new string[] { "NSSL" });
                _keywords.Add("pegasus_vendor_invoice", new string[] { "PCSL" });
            }
            else if (Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
            {
                //US
                _keywords.Add("one_vendor_invoice", new string[] { "ONE", "OCEAN", "NETWORK", "EXPRESS" });
                _keywords.Add("hmm_vendor_invoice", new string[] { "HDMU", "HMM" });
            }
            else
            {
                //EU
                _keywords.Add("one_vendor_invoice", new string[] { "ONE", "OCEAN", "NETWORK", "EXPRESS", "INVOICE" });
                _keywords.Add("hapag_vendor_invoice", new string[] { "HAPAG-LLOYD", "Hapag-Lloyd", "INVOICE" });
                _keywords.Add("hmm_vendor_invoice", new string[] { "Hyundai Merchant Marine", "HMM" });
                _keywords.Add("maersk_vendor_invoice", new string[] { "Maersk", "A/S" });
                _keywords.Add("cosco_vendor_invoice", new string[] { "COSCO", "SHIPPING", "LINES", "COSCO SHIPPING LINES" });
            }

            _keywords.Add("default", new string[] { "" });

            #endregion

            foreach (KeyValuePair<string, string[]> item in _keywords)
            {
                foreach (string keyword in item.Value)
                {
                    _totalKeywords.Add(keyword);
                }
            }
        }

        public void DetectText(string filename)
        {
            var client = ImageAnnotatorClient.Create();
            var image = Google.Cloud.Vision.V1.Image.FromFile(filename);
            var response = client.DetectText(image);

            string temp = response[0].Description.Replace("\n", "\r\n");
            string classifiedDocument = ClassifyDocument(temp);

            Console.WriteLine(classifiedDocument);

            if (pdfViewer1.GetSelectionContent().Image != null)
            {
                Clipboard.SetDataObject(temp);
            }
            else
            {
                if (classifiedDocument == "default")
                {
                    //학습한 서류 아님
                }
                else
                {
                    GetKeyFeatures(classifiedDocument, response);

                    DataValidCheck();

                    if (aTextEdit_VendorRefNo.Text != string.Empty)
                    {
                        DoSearch2(Global.BizCode, aTextEdit_VendorRefNo.Text);

                        if (aTextEdit_VendorInvNo.Text != string.Empty
                            && aDateEdit_VendorDueDate.Text != string.Empty
                            //&& aDateEdit_VendorReceivedDate.Text != string.Empty
                            && aNumericText_TotalAmount.Text != "0.00")
                        {
                            AButton_Reflact_Click(null, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 서류 내용을 보고 어떤 업체인지, 어떤 유형의 서류인지 분류하는 메소드
        /// 1. 특정 키워드 빈도수 체크 (업체명, 주소, INVOICE, PACKING 등)
        /// 2. ...
        /// </summary>
        /// <param name="AllDetectText"></param>
        /// <returns></returns>
        public string ClassifyDocument(string AllDetectText)
        {
            string result = string.Empty;

            // 텍스트의 키워드 빈도수 체크
            var frequencyList = AllDetectText.Split(new string[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
               .Select(c => c)
               .Where(c => _totalKeywords.Contains(c))
               .GroupBy(c => c)
               .Select(g => new { Word = g.Key, Count = g.Count() })
               .OrderByDescending(g => g.Count)
               .ThenBy(g => g.Word);
            Dictionary<string, int> dict = frequencyList.ToDictionary(d => d.Word, d => d.Count);

            // 체크된 키워드 별 빈도수를 통해 서류 분류 의사결정
            if (dict.Count != 0)
            {
                Dictionary<string, double> temp = _documentFrequency.ToDictionary(entry => entry.Key, entry => entry.Value);
                foreach (KeyValuePair<string, double> item in temp)
                {
                    double total = 0.0;
                    double seperate_total = 0.0;

                    string[] keywords = _keywords[item.Key];

                    foreach (KeyValuePair<string, int> keyword in dict)
                    {
                        total += keyword.Value;
                        if (keywords.Contains(keyword.Key.ToUpper()))
                        {
                            seperate_total += keyword.Value;
                        }
                    }

                    if (item.Key != "default")
                    {
                        if (total == 0.0) _documentFrequency[item.Key] = 0.0;
                        else _documentFrequency[item.Key] = (seperate_total / keywords.Length) * (seperate_total / total);
                    }
                }
            }
            result = _documentFrequency.OrderByDescending(x => x.Value).FirstOrDefault().Key;

            return result;
        }

        public void GetKeyFeatures(string gubun, IReadOnlyList<EntityAnnotation> response)
        {
            switch (gubun)
            {
                #region - one_vendor_invoice
                case "one_vendor_invoice":
                    for (int i = 1; i < response.Count; i++)
                    {
                        int x1 = response[i].BoundingPoly.Vertices[0].X;
                        int x2 = response[i].BoundingPoly.Vertices[2].X;
                        int y1 = response[i].BoundingPoly.Vertices[0].Y;
                        int y2 = response[i].BoundingPoly.Vertices[2].Y;

                        //INVOICE NUMBER
                        if (response[i].Description.IndexOf("INVOICE") != -1)
                        {
                            if (i + 1 < response.Count && response[i + 1].Description.IndexOf("NUMBER") != -1)
                            {
                                //INVOICE NUMBER
                                Console.WriteLine(response[i].BoundingPoly.Vertices);
                                //Console.WriteLine(GetText(response, x1, x2, y1, y2, 5, "down", 15));
                                string result = GetText(response, x1, response[i + 1].BoundingPoly.Vertices[2].X, y1, y2, 7, "down", 15);
                                Console.WriteLine(result);
                                aTextEdit_VendorInvNo.Text = result;
                                //richTextBox1.Text += "INVOICE NUMBER : " + GetText(response, x1, response[i + 1].BoundingPoly.Vertices[2].X, y1, y2, 7, "down", 15);
                                //richTextBox1.Text += Environment.NewLine;
                            }
                        }
                        else if (response[i].Description.IndexOf("ISSUE") != -1 || response[i].Description.IndexOf("1SSUE") != -1)
                        {
                            if (i + 1 < response.Count && response[i + 1].Description.IndexOf("DATE") != -1)
                            {
                                //CUSTOMER'S REFERENCE
                                Console.WriteLine(response[i].BoundingPoly.Vertices);
                                //Console.WriteLine(GetText(response, x1, x2, y1, y2, 5, "down", 15));
                                string date = GetText(response, x1, response[i + 1].BoundingPoly.Vertices[2].X, y1, y2, 7, "down", 15);
                                //Console.WriteLine(date);

                                string result = PostValidCheck("DATE", date);
                                Console.WriteLine(result);
                                aDateEdit_VendorReceivedDate.Text = result;
                                //richTextBox1.Text += "DUE DATE : " + result;
                                //richTextBox1.Text += Environment.NewLine;
                            }
                        }
                        else if (response[i].Description.IndexOf("DUE") != -1)
                        {
                            if (i + 1 < response.Count && response[i + 1].Description.IndexOf("DATE") != -1)
                            {
                                //CUSTOMER'S REFERENCE
                                Console.WriteLine(response[i].BoundingPoly.Vertices);
                                //Console.WriteLine(GetText(response, x1, x2, y1, y2, 5, "down", 15));
                                string date = GetText(response, x1, response[i + 1].BoundingPoly.Vertices[2].X, y1, y2, 7, "down", 15);
                                //Console.WriteLine(date);

                                string result = PostValidCheck("DATE", date);
                                Console.WriteLine(result);
                                aDateEdit_VendorDueDate.Text = result;
                                //richTextBox1.Text += "DUE DATE : " + result;
                                //richTextBox1.Text += Environment.NewLine;
                            }
                        }
                        else if (response[i].Description.IndexOf("CUSTOMER") != -1)
                        {
                            if (i + 1 < response.Count && response[i + 1].Description.IndexOf("REFERENCE") != -1)
                            {
                                //CUSTOMER'S REFERENCE
                                Console.WriteLine(response[i].BoundingPoly.Vertices);
                                //Console.WriteLine(GetText(response, x1, x2, y1, y2, 5, "down", 15));
                                string result = GetText(response, x1, response[i + 1].BoundingPoly.Vertices[2].X, y1, y2, 7, "down", 15);
                                Console.WriteLine(result);
                                //richTextBox1.Text += "CUSTOMER'S REFERENCE : " + GetText(response, x1, response[i + 1].BoundingPoly.Vertices[2].X, y1, y2, 7, "down", 15);
                                //richTextBox1.Text += Environment.NewLine;
                            }
                        }
                        else if (response[i].Description.IndexOf("GRAND") != -1)
                        {
                            if (i + 1 < response.Count && response[i + 1].Description.IndexOf("TOTAL") != -1)
                            {
                                if (i + 2 < response.Count && response[i + 2].Description.IndexOf("AMOUNT") != -1)
                                {
                                    //GRAND TOTAL AMOUNT
                                    Console.WriteLine(response[i].BoundingPoly.Vertices);
                                    double result = 0;
                                    Double.TryParse(GetText(response, x1, x2, y1, y2, 25, "right", 580), out result);
                                    Console.WriteLine(result);
                                    aNumericText_TotalAmount.Text = result.ToString();
                                    //richTextBox1.Text += "GRAND TOTAL AMOUNT : " + GetText(response, x1, x2, y1, y2, 25, "right", 580);
                                    //richTextBox1.Text += Environment.NewLine;
                                }
                            }
                        }
                        else if (i < response.Count && response[i].Description.IndexOf("BL") != -1)
                        {
                            if (i + 1 < response.Count && response[i + 1].Description.IndexOf("NUMBER") != -1)
                            {
                                //BKG / BL NUMBER
                                Console.WriteLine(response[i].BoundingPoly.Vertices);
                                string result = GetText(response, x1, x2, y1, y2, 10, "right", 275);
                                Console.WriteLine(result);
                                aTextEdit_VendorRefNo.Text = "ONEY" + result;
                                //aTextEdit_RefNo.Text = "ONEYGOAA08139400";

                            }
                        }
                    }

                    break;
                #endregion

                #region - hapag_vendor_invoice
                case "hapag_vendor_invoice":

                    break;
                    #endregion
            }
        }

        public void GetKeyFeatures_EU(string gubun, string pageText, int pageCnt)
        {
            string[] temp = null;
            gubun.Split('/');
            switch (gubun)
            {
                #region - one_vendor_invoice

                case "one_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].IndexOf("INVOICE NUMBER ISSUE DATE TO") != -1)
                        {
                            string[] value = temp[i + 1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorInvNo.Text = value[0];
                            aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE", value[1]);
                        }
                        else if (temp[i].IndexOf("CUSTOMER CODE DUE DATE") != -1)
                        {
                            string[] value = temp[i + 1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aDateEdit_VendorDueDate.Text = PostValidCheck("DATE", value[1]);
                        }
                        else if (temp[i].IndexOf("BKG/BL NUMBER") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (Global.BizCode == "PAR")
                            {
                                aTextEdit_VendorRefNo.Text = "ONEY" + value[2];
                            }
                            else
                            {
                                aTextEdit_VendorRefNo.Text = "ONEY" + value[2];
                            }
                        }
                        else if (temp[i].IndexOf("GRAND TOTAL AMOUNT IN") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (value.Length > 5)
                            {
                                aNumericText_TotalAmount.Text = value[5];
                            }
                        }
                    }

                    break;

                #endregion

                #region - hapag_vendor_invoice

                case "hapag_vendor_invoice":

                    if (Global.BizCode == "MIL")
                    {
                        temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (temp[i].IndexOf("N O T A C R E D I T O NR.:") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[12];
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE2", value[13] + value[14] + value[15]);
                            }
                            else if (temp[i].IndexOf("F A T T U R A NR.:") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[8];
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE2", value[9] + value[10] + value[11]);
                            }
                            else if (temp[i].IndexOf("TOTALE") != -1 && temp[i].IndexOf("EUR") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                value[1] = value[1].Replace(",", "/");
                                aNumericText_TotalAmount.Text = value[1].Replace(".", ",").Replace("/", ".");
                            }
                            else if (temp[i].IndexOf("DATA PAGAMENTO") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE2", value[2] + value[3] + value[4]);
                            }
                            else if (temp[i].IndexOf("SWB-NR") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = value[1];
                            }
                        }
                    }
                    else if (Global.BizCode == "PAR")
                    {
                        temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (temp[i].IndexOf("F A C T U R E NO.:") != -1 && pageCnt == 1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[8];
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE2", value[9] + value[10] + value[11]);

                                DateTime date2 = DateTime.ParseExact(PostValidCheck("DATE2", value[9] + value[10] + value[11]), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                aDateEdit_VendorDueDate.Text = date2.AddDays(15).ToShortDateString();
                            }
                            else if (temp[i].IndexOf("TOTAL T.T.C.") != -1 && temp[i].IndexOf("EUR") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aNumericText_TotalAmount.Text = value[2];
                            }
                            else if (temp[i].IndexOf("SWB-NO.") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = value[1];
                            }
                        }
                    }
                    else
                    {
                        temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (temp[i].IndexOf("R E C H N U N G NR.:") != -1 && pageCnt == 1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[9];
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE3", value[10]);
                            }
                            else if ((temp[i].IndexOf("BRUTTO") != -1 || temp[i].IndexOf("SUMME") != -1) && temp[i].IndexOf("EUR") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                value[1] = value[1].Replace(",", "/");
                                aNumericText_TotalAmount.Text = value[1].Replace(".", ",").Replace("/", ".");
                            }
                            else if (temp[i].IndexOf("ZAHLUNGSFAELLIGKEIT:") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE3", value[1]);
                            }
                            else if (temp[i].IndexOf("SWB-NR") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = value[1];
                            }
                        }
                    }

                    break;

                #endregion

                #region - hmm_vendor_invoice

                case "hmm_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].IndexOf("Number :") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorInvNo.Text = value[2];
                        }
                        else if (temp[i].IndexOf("Invoice Date") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE", value[7] + value[8] + value[9]);
                        }
                        else if (temp[i].IndexOf("Due Date") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                            if (Global.BizCode == "HAM")
                            {
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE", value[5] + value[6] + value[7]);
                            }
                            else
                            {
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE", value[8] + value[9] + value[10]);
                            }
                        }
                        else if (temp[i].IndexOf("B/L No") != -1)
                        {
                            if (temp[i + 4].IndexOf("HOE") != -1 && Global.BizCode == "HAM")
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = value[3];
                            }
                            else
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = "HDMU" + value[3];
                            }
                        }
                        else if (temp[i].IndexOf("TOTAL") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aNumericText_TotalAmount.Text = value[2];
                        }
                    }

                    break;

                #endregion

                #region - maersk_vendor_invoice

                case "maersk_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (Global.BizCode == "HAM")
                        {
                            if (temp[i].IndexOf("EXPORT RECHNUNG") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[2];
                            }
                            else if (temp[i].IndexOf("Rechnungsdatum") != -1 && pageCnt == 1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE", value[1].Replace(".", ""));
                            }
                            else if (temp[i].IndexOf("Faelligkeitsdatum") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE", value[1].Replace(".", ""));
                            }
                            else if (temp[i].IndexOf("Bill of Lading Nummer") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = value[4];
                            }
                            else if (temp[i].IndexOf("Faelliger Betrag") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                value[2] = value[2].Replace(",", "/");
                                aNumericText_TotalAmount.Text = value[2].Replace(".", ",").Replace("/", ".");
                            }
                        }
                        else if (Global.BizCode == "PAR")
                        {
                            if (temp[i].IndexOf("Facture export") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[2];
                            }
                            else if (temp[i].IndexOf("Date de Facture") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE", value[3].Replace(".", ""));
                            }
                            else if (temp[i].IndexOf("Date d'Échéance") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE", value[2].Replace(".", ""));
                            }
                            else if (temp[i].IndexOf("Connaissement:") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = "MAEU" + value[3];
                            }
                            else if (temp[i].IndexOf("Montant Dû") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                value[2] = value[2].Replace(",", "/");
                                aNumericText_TotalAmount.Text = value[2].Replace(".", ",").Replace("/", ".");
                            }
                        }
                    }

                    break;

                #endregion

                #region - cosco_vendor_invoice

                case "cosco_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (Global.BizCode == "HAM")
                        {
                            if (temp[i].IndexOf("INVOICE NO.") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[1].Replace(" ", "");
                            }
                            else if (temp[i].IndexOf("ISSUE DATE") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE", value[3] + value[4] + value[5]);
                            }
                            else if (temp[i].IndexOf("DUE DATE") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE", value[3] + value[4] + value[5]);
                            }
                            else if (temp[i].IndexOf("BILL OF LADING NO") != -1)
                            {
                                string[] value = temp[i + 1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = "COSU" + value[0];
                            }
                            else if (temp[i].IndexOf("AMOUNT DUE") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aNumericText_TotalAmount.Text = value[3];
                            }
                        }
                        else if (Global.BizCode == "PAR")
                        {
                            if (temp[i].IndexOf("INVOICE NO.") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[1].Replace(" ", "");
                            }
                            else if (temp[i].IndexOf("DATE :") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE", value[2] + value[3] + value[4]);
                            }
                            else if (temp[i].IndexOf("DATE D'ECHEANCE") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE", value[3] + value[4] + value[5]);
                            }
                            else if (temp[i].IndexOf("CONNAISSEMENT") != -1)
                            {
                                string[] value = temp[i + 1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = value[0];
                            }
                            else if (temp[i].IndexOf("MONTANT DU") != -1)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aNumericText_TotalAmount.Text = value[3];
                            }
                        }
                        else if (Global.BizCode == "MIL")
                        {
                            if (temp[i].IndexOf("BL NUMBER AMOUNT DUE EUR USD") != -1)
                            {
                                string[] value = temp[i + 2].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorInvNo.Text = value[0].Replace(" ", "");

                                string[] value2 = temp[i + 3].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorReceivedDate.Text = PostValidCheck("DATE", value2[0] + value2[1] + value2[2]);

                                string[] value3 = temp[i + 4].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aDateEdit_VendorDueDate.Text = PostValidCheck("DATE", value3[0] + value3[1] + value3[2]);
                            }
                            else if (i == temp.Length - 3)
                            {
                                string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = "COSU" + value[0].Substring(0,10);
                                aNumericText_TotalAmount.Text = value[1];
                            }
                        }
                    }

                    break;

                    #endregion
            }
        }

        public void GetKeyFeatures_JP(string gubun, string pageText, int pageCnt)
        {
            string[] temp = null;
            string exRate = string.Empty;
            gubun.Split('/');
            switch (gubun)
            {
                #region - kmtc_vendor_invoice

                case "kmtc_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].IndexOf("KMTC") != -1 && temp[i].IndexOf("JAPAN") == -1 && temp[i].IndexOf("AIR-SEA") == -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorInvNo.Text = value[0];
                            aTextEdit_VendorMBLNo.Text = value[0];

                            string[] value2 = temp[i + 3].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aDateEdit_VendorReceivedDate.Text = value2[0].Replace(".", "");
                            aDateEdit_VendorDueDate.Text = PostValidCheck("DATE_JP", value2[0].Replace(".", ""));
                        }
                        else if (temp[i].IndexOf("JPY") != -1 || temp[i].IndexOf("USD") != -1)
                        {
                            string[] value = temp[i + 1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (value.Length == 1)
                            {
                                //Total Amount
                                aNumericText_TotalAmount.Text = value[0];

                                //Container No. 
                                string[] value2 = temp[i + 4].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                aTextEdit_VendorRefNo.Text = value2[0];
                            }

                            //Grid add
                            string[] value3 = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (value3[0] == "OCEAN" || value3[0] == "T.H.C." || value3[0] == "DOCUMENT" || value3[0] == "SEAL"
                                || value3[0] == "COST" || value3[0] == "LSS" || value3[0] == "ADVANCE")
                            {
                                SettingFreight_jp(gubun, value3, "");
                            }
                        }
                    }

                    break;

                #endregion

                #region - sitc_vendor_invoice

                case "sitc_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    //환율 구하기용
                    for (int i = 40; i < temp.Length; i++)
                    {
                        if (temp[i].Contains("EX."))
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            exRate = value[1].Replace(")", "");
                        }
                    }

                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (i == 2 && temp[2].Contains("SIT") && pageCnt == 1)
                        {
                            string[] value = temp[2].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorInvNo.Text = value[0];
                            aTextEdit_VendorMBLNo.Text = value[0];
                        }
                        else if (i == 3 && temp[3].Contains("SIT"))
                        {
                            string[] value = temp[3].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorRefNo.Text = value[0];
                        }
                        else if ((temp[i].IndexOf("JPY") != -1 || temp[i].IndexOf("USD") != -1) && temp[i].IndexOf(":") == -1)
                        {
                            //Grid add
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (value[0] == "OF" || value[0] == "JPDOC" || value[0] == "JPTHC" || value[0] == "AFS" || value[0] == "JPSF")
                            {
                                SettingFreight_jp(gubun, value, exRate);
                            }
                            else
                            {
                                //총 합계로 인식
                                if (value.Length == 2)
                                {
                                    aNumericText_TotalAmount.Text = value[1];
                                }
                            }
                        }
                        else if (temp[i].Contains("JPY:"))
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aNumericText_TotalAmount.Text = value[3].Substring(4);
                        }

                        if (i == temp.Length - 1)
                        {
                            aDateEdit_VendorReceivedDate.Text = DateTime.Now.ToString("yyyyMMdd");
                            aDateEdit_VendorDueDate.Text = PostValidCheck("DATE_JP", DateTime.Now.ToString("yyyyMMdd"));
                        }
                    }

                    break;

                #endregion

                #region - heung_vendor_invoice / sinokor_vendor_invoice

                case "heung_vendor_invoice":
                case "sinokor_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].IndexOf("InvoiceーDate") != -1 || temp[i].IndexOf("Invoice Date") != -1 || temp[i].IndexOf("ISSUED DATE") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                            aDateEdit_VendorReceivedDate.Text = value[1].Replace("-", "").Trim();
                            aDateEdit_VendorDueDate.Text = PostValidCheck("DATE_JP", value[1].Replace("-", "").Trim());
                        }
                        else if (temp[i].IndexOf("SUB TOTAL") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorRefNo.Text = value[4];
                            aTextEdit_VendorInvNo.Text = value[4];
                            aTextEdit_VendorMBLNo.Text = value[4];
                            aNumericText_TotalAmount.Text = value[7];
                        }
                        //else if (temp[i].IndexOf("INVOICE TOTAL AMOUNT") != -1)
                        //{
                        //    string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        //    aNumericText_TotalAmount.Text = value[5];
                        //}
                        else if (temp[i].IndexOf("JPY") != -1 || temp[i].IndexOf("USD") != -1)
                        {
                            //Grid add
                            string[] value = temp[i].Split(new string[] { "JPY", "USD" }, StringSplitOptions.RemoveEmptyEntries);
                            if (value[0].Contains("OCEAN") || value[0].Contains("LOADING") || value[0].Contains("DOCUMENTATION") || value[0].Contains("Seal") || value[0].Contains("BUNKER"))
                            {
                                SettingFreight_jp(gubun, value, "");
                            }
                        }
                    }

                    break;

                #endregion

                #region - namsung_vendor_invoice / pegasus_vendor_invoice

                case "namsung_vendor_invoice":
                case "pegasus_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    //환율 구하기용
                    for (int i = 30; i < temp.Length; i++)
                    {
                        if (temp[i].Contains("ex.RATE"))
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            exRate = value[3];
                        }
                    }

                    for (int i = 0; i < temp.Length; i++)
                    {
                        if ((temp[i].IndexOf("NSSL") != -1 || temp[i].IndexOf("PCSL") != -1) && pageCnt == 1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorInvNo.Text = value[0];
                            aTextEdit_VendorMBLNo.Text = value[0];
                        }
                        else if (temp[i].IndexOf("Booking") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorRefNo.Text = value[2];
                        }
                        else if (temp[i].IndexOf("JPY") != -1 || temp[i].IndexOf("USD") != -1)
                        {
                            //Grid add
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (value[0] == "OCEAN" || value[0] == "DOCUMENT" || value[0] == "SEAL" || value[0] == "THC")
                            {
                                SettingFreight_jp(gubun, value, exRate);
                            }
                        }
                        else if (temp[i].IndexOf("TOTAL") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aNumericText_TotalAmount.Text = value[2];
                        }

                        if (i == temp.Length - 1)
                        {
                            aDateEdit_VendorReceivedDate.Text = DateTime.Now.ToString("yyyyMMdd");
                            aDateEdit_VendorDueDate.Text = PostValidCheck("DATE_JP", DateTime.Now.ToString("yyyyMMdd"));
                        }
                    }

                    break;

                #endregion
            }
        }

        public void GetKeyFeatures_US(string gubun, string pageText, int pageCnt)
        {
            string[] temp = null;
            gubun.Split('/');
            switch (gubun)
            {
                #region - one_vendor_invoice

                case "one_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].IndexOf("ONEY") != -1 && pageCnt == 1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            aTextEdit_VendorInvNo.Text = value[1];
                            aTextEdit_VendorRefNo.Text = value[1];
                        }
                        else if (temp[i].IndexOf("TOTAL") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (value.Length == 5)
                            {
                                aNumericText_TotalAmount.Text = value[2];
                            }
                            else if (value.Length == 10)
                            {
                                aNumericText_TotalAmount.Text = value[6];
                            }
                        }

                        if (i == temp.Length - 1)
                        {
                            aDateEdit_VendorReceivedDate.Text = DateTime.Now.ToString("yyyyMMdd");
                            aDateEdit_VendorDueDate.Text = PostValidCheck("DATE_JP", DateTime.Now.ToString("yyyyMMdd"));
                        }
                    }

                    break;

                #endregion

                #region - hmm_vendor_invoice

                case "hmm_vendor_invoice":

                    temp = pageText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i].IndexOf("HDMU") != -1)
                        {
                            string[] value = temp[i + 1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            if (temp[i] == "HDMU")
                            {
                                aTextEdit_VendorInvNo.Text = "HDMU" + value[0];
                                aTextEdit_VendorRefNo.Text = "HDMU" + value[0];
                            }
                        }
                        else if (temp[i].IndexOf("USD") != -1)
                        {
                            string[] value = temp[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                            if (value.Length < 5)
                            {
                                aNumericText_TotalAmount.Text = value[1];
                            }
                        }

                        if (i == temp.Length - 1)
                        {
                            aDateEdit_VendorReceivedDate.Text = DateTime.Now.ToString("yyyyMMdd");
                            aDateEdit_VendorDueDate.Text = PostValidCheck("DATE_JP", DateTime.Now.ToString("yyyyMMdd"));
                        }
                    }

                    break;

                    #endregion
            }
        }

        /// <summary>
        /// 상대적 위치로 특정값을 찾는 메소드
        /// </summary>
        /// <param name="response"></param>
        /// <param name="px1"></param>
        /// <param name="px2"></param>
        /// <param name="py1"></param>
        /// <param name="py2"></param>
        /// <param name="diff"></param>
        /// <param name="type"></param>
        /// <param name="gap"></param>
        /// <returns></returns>
        public string GetText(IReadOnlyList<EntityAnnotation> response, int px1, int px2, int py1, int py2, int diff, string type, int gap)
        {
            string result = string.Empty;

            if (type == "down")
            {
                py1 += gap; py2 += gap;
            }
            else if (type == "right")
            {
                px2 += gap;
            }

            for (int i = 1; i < response.Count; i++)
            {
                int x1 = response[i].BoundingPoly.Vertices[0].X;
                int x2 = response[i].BoundingPoly.Vertices[2].X;
                int y1 = response[i].BoundingPoly.Vertices[0].Y;
                int y2 = response[i].BoundingPoly.Vertices[2].Y;

                if (type == "down")
                {
                    if (((px1 - diff) <= x1 && x1 <= (px2 + diff)) /*&& ((px2 - diff) <= x2 && x2 <= (px2 + diff))*/
                     && ((py1 - diff) <= y1 && y1 <= (py1 + diff)) && ((py2 - diff) <= y2 && y2 <= (py2 + diff)))
                    {
                        result += response[i].Description;// + "$^^$";
                        //break;
                    }
                }
                else if (type == "right")
                {
                    if (/*((px1 - diff) <= x1 && x1 <= (px1 + diff)) && */((px2 - diff) <= x2 && x2 <= (px2 + diff))
                     && ((py1 - diff) <= y1 && y1 <= (py1 + diff)) && ((py2 - diff) <= y2 && y2 <= (py2 + diff)))
                    {
                        result += response[i].Description;// + "$^^$";
                        //break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// OCR로 인식한 값이 우리가 원하는 형태로 인식됐는지 체크하고 보정하는 메소드
        /// </summary>
        /// <param name="gubun"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string PostValidCheck(string gubun, string value)
        {
            string result = string.Empty;
            string month = string.Empty;
            string day = string.Empty;
            string year = string.Empty;
            string[] temp = null;

            value = value.Replace("$^^$", "");

            switch (gubun)
            {
                // 05Jun2020 형태
                case "DATE":
                    // 영문 월을 체크
                    month = CheckMonthToChar(value);
                    if (month == string.Empty) break;

                    temp = value.ToUpper().Split(new string[] { month }, StringSplitOptions.RemoveEmptyEntries);

                    day = CheckDay(temp[0]);
                    year = CheckYear(temp[1]);
                    month = GetMonthToNumber(month);

                    result = year + month + day;

                    break;

                // JUNE5,2020 형태
                case "DATE2":
                    // 영문 월을 체크
                    month = CheckMonthToChar(value);
                    if (month == string.Empty) break;

                    temp = value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    day = CheckDay(Regex.Replace(temp[0], @"\D", ""));
                    year = CheckYear(temp[1]);
                    month = GetMonthToNumber(month);

                    result = year + month + day;

                    break;

                // 14.07.2020 형태
                case "DATE3":

                    temp = value.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

                    day = CheckDay(temp[0]);
                    month = temp[1];
                    year = CheckYear(temp[2]);

                    result = year + month + day;

                    break;

                case "DATE_JP":

                    DateTime dueDate = DateTime.ParseExact(value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture); //Convert.ToDateTime(value); //EX 20200914
                    dueDate = dueDate.AddMonths(1);
                    string lastDay = A.GetString(DateTime.DaysInMonth(dueDate.Year, dueDate.Month));

                    result = dueDate.ToString("yyyyMM") + lastDay;

                    break;
            }

            return result;
        }

        public string CheckYear(string value)
        {
            string result = string.Empty;

            // 숫자인지 체크
            int i = -1;
            bool check = int.TryParse(value, out i);
            if (check)
            {
                //지금 년도를 기준으로 +-5년 사이에 있지 않을까?
                int year = -1;
                int.TryParse(DateTime.Now.ToString("yyyy"), out year);
                if (year - 5 <= i && i <= year + 5) result = i.ToString();
                else
                {
                    // 유형 판단 필요
                }
            }
            else
            {
                // 숫자가 아닌 경우에는 경험을 통한 매핑으로 처리
            }

            return result;
        }

        public string CheckDay(string value)
        {
            string result = string.Empty;

            // 숫자인지 체크
            int i = -1;
            bool check = int.TryParse(value, out i);
            if (check)
            {
                if (i <= 31)
                {
                    // 월 마다 일 수 차이는 있지만 정상이라고 판단
                    if (i < 10) result = "0" + i;
                    else result = i.ToString();
                }
                else
                {
                    // 유형 판단 필요 (앞에 1이 붙었는지 등)

                }
            }
            else
            {
                // 숫자가 아닌 경우에는 경험을 통한 매핑으로 처리 (oz => 05) 이런 개념으로
                foreach (KeyValuePair<string, string> item in _mappingDay)
                {
                    if (item.Key.ToUpper() == value.ToUpper())
                    {
                        result = item.Value;
                    }
                }
            }

            return result;
        }

        public string GetMonthToNumber(string date)
        {
            string result = string.Empty;

            string[] month = new string[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN"
                                           ,"JUL", "AUG", "SEP", "OCT", "NOV", "DEC"};

            for (int i = 0; i < month.Length; i++)
            {
                if (date.ToUpper().IndexOf(month[i]) != -1)
                {
                    if ((i + 1) < 10) result = "0" + (i + 1);
                    else result = (i + 1).ToString();
                    break;
                }
            }

            return result;
        }

        public string CheckMonthToChar(string date)
        {
            string result = string.Empty;

            string[] month = new string[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUN"
                                           ,"JUL", "AUG", "SEP", "OCT", "NOV", "DEC"};

            for (int i = 0; i < month.Length; i++)
            {
                if (date.ToUpper().IndexOf(month[i]) != -1)
                {
                    result = month[i];
                    break;
                }
            }

            return result;
        }

        #endregion

        private void DoSearch(string cdBiz, string noSlipOrder)
        {
            DataSet ds = _D.Search(new object[] { Global.FirmCode, cdBiz, noSlipOrder });
            if (ds.Tables[0].Rows.Count != 0)
            {
                _HEADER.SetDataTable(ds.Tables[0]);
                aGrid_MBLContainer.Binding(ds.Tables[1]);

                MenuKey = aTextEdit_ShipmentNo.Text;

                aLabel_MappingNo.Visible = false;
                aLabel_MappingYes.Visible = true;
            }
            else
            {
                aLabel_MappingNo.Visible = true;
                aLabel_MappingYes.Visible = false;
            }

            if (Global.BizCode == "TYO")
            {
                foreach (DataRow row in (aGrid_Freight.DataSource as DataTable).Rows)
                {
                    row["TM_INVOICE_POST"] = _HEADER.CurrentRow["TM_INVOICE_POST"];
                }
            }

            ////기존 AP 조회용도(필요 시 주석 해제)
            //SettingAPRequest(_HEADER.CurrentRow["CD_BIZ"].ToString(), _HEADER.CurrentRow["NO_SLIP_INVOICE"].ToString());

            //// 조회값 셋팅
            //aDateEdit_PostDate.Text = A.GetString(_HEADER.CurrentRow["TM_INVOICE_POST"]);
            //aCodeText_Vendor.CodeValue = A.GetString(_HEADER.CurrentRow["CD_PARTNER_CARRIER"]);
            //aCodeText_Vendor.CodeName = A.GetString(_HEADER.CurrentRow["NM_PARTNER_CARRIER"]);
            //aMemoEdit_PaidToAddr.Text = A.GetString(_HEADER.CurrentRow["DC_PARTNER_CARRIER_ADDR"]);
        }

        private void DoSearch2(string cdBiz, string noBl)
        {
            DataSet ds = _D.Search2(new object[] { Global.FirmCode, cdBiz, noBl });
            if (ds.Tables[0].Rows.Count != 0)
            {
                _HEADER.SetDataTable(ds.Tables[0]);
                aGrid_MBLContainer.Binding(ds.Tables[1]);

                MenuKey = aTextEdit_ShipmentNo.Text;

                aLabel_MappingNo.Visible = false;
                aLabel_MappingYes.Visible = true;

                aTextEdit_RequestNo.Text = string.Empty;
                aTextEdit_NM_STATUS.Text = string.Empty;
                aTextEdit_AttachInvFile.Text = string.Empty;

                //aCodeText_Vendor.SetCodeNameNValue(A.GetString(_HEADER.CurrentRow["CD_PARTNER_BILL_TO"]), A.GetString(_HEADER.CurrentRow["NM_PARTNER_BILL_TO"]));

                //기존 AP 조회용도
                string noInvoiceRel = _HEADER.CurrentRow["NO_INVOICE_REL"].ToString();
                string cdPartnerCarrier = _HEADER.CurrentRow["CD_PARTNER_CARRIER"].ToString();

                _noInvoiceAp = SettingNoInvoiceAp(noInvoiceRel, cdPartnerCarrier);

                if (Global.BizCode == "TYO")
                {
                    foreach (DataRow row in (aGrid_Freight.DataSource as DataTable).Rows)
                    {
                        row["TM_INVOICE_POST"] = _HEADER.CurrentRow["TM_INVOICE_POST"];
                    }
                }
            }
            else
            {
                aLabel_MappingNo.Visible = true;
                aLabel_MappingYes.Visible = false;

                aTextEdit_RequestNo.Text = string.Empty;
                aTextEdit_NM_STATUS.Text = string.Empty;
            }
        }

        private void DoSave()
        {
            string noInvoice = A.GetString(aTextEdit_InvoiceNo.Text);
            string noBl = A.GetString(aTextEdit_VendorMBLNo.Text);
            string noSlipInvoice = A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE"]);
            string noSlipBl = A.GetString(_HEADER.CurrentRow["NO_INVOICE_REL"]);

            if (noSlipInvoice == string.Empty)
            {
                noSlipInvoice = A.GetSlipNoNew(Global.BizCode, "INT", 6);
                _noSlipInvoice = noSlipInvoice;
                _HEADER.CurrentRow["NO_SLIP_INVOICE"] = noSlipInvoice;
                _HEADER.CurrentRow["CD_BIZ"] = Global.BizCode;
            }

            // ## AP의 Invoice No.는 직접 키인함 ##
            //if (noInvoice == string.Empty)
            //{
            //    noInvoice = A.GetSlipNo("INT", 2);
            //    aTextEdit_InvoiceNo.Text = noInvoice;
            //}

            DataTable dtInvoiceChanges = _HEADER.GetChanges();
            DataTable dtFreightChanges = aGrid_Freight.GetChanges();

            if (dtInvoiceChanges == null && dtFreightChanges == null)
            {
                ShowMessageBoxA("Do not have changed content.", MessageType.Information);
                return;
            }

            List<string> list = new List<string>();
            list.Add(A.GetString(_HEADER.CurrentRow["CD_BIZ"]));
            list.Add(A.GetString(noSlipInvoice));
            list.Add(A.GetString(_HEADER.CurrentRow["NO_INVOICE_REL"]));
            //list.Add(A.GetString(_HEADER.CurrentRow["TM_INVOICE_POST"]));
            list.Add(A.GetString(aLookUpEdit_Currency.EditValue));

            if (!_D.Save(dtInvoiceChanges, dtFreightChanges, list))
            {
                ShowMessageBoxA("It could not have been saved successfully.", MessageType.Error);
                return;
            }
            else
            {
                OnSaveFile();
                _D.UpdateInvoiceYn(Global.BizCode, noSlipInvoice);

                if (Global.BizCode == "TYO")
                {
                    //일본은 AP 생성 전에 MBL 기입하지 않기 때문에, AP 생성 하면서 같이 업데이트 처리
                    _D.UpdateMbl(Global.BizCode, noSlipBl, noBl);
                    aTextEdit_MBLNo.Text = noBl;
                }
            }

            //aTaskHelper.GetTask(_HEADER, "M_INT_INVOICE_AP");

            ShowMessageBoxA("It was successfully saved.", MessageType.Information);
            MenuKey = aTextEdit_InvoiceNo.Text == string.Empty ? A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE"]) : aTextEdit_InvoiceNo.Text;
        }

        private void SearchInvoice(string cdBiz, string noSlipInvoice, string fgRegType)
        {
            DataSet ds = _D.Search(new object[] { Global.FirmCode, cdBiz, noSlipInvoice, fgRegType });
            _HEADER.SetDataTable(ds.Tables[0]);
            aGrid_Freight.Binding(ds.Tables[1]);
            _fgShippingMode = A.GetString(ds.Tables[0].Rows[0]["FG_SHIPPING_MODE"]);

            //aTextEdit_CD_INVOICE_STATUS.Text = A.GetString(ds.Tables[0].Rows[0]["CD_INVOICE_STATUS"]);
            MenuKey = aTextEdit_InvoiceNo.Text == string.Empty ? A.GetString(_HEADER.CurrentRow["NO_SLIP_INVOICE"]) : aTextEdit_InvoiceNo.Text;
        }

        private void SettingOfProgress(string menuId, string cdBiz, string noSlipProgress)
        {

            DataSet dsProgress = _D.SearchProgress(new object[] { Global.FirmCode, cdBiz, noSlipProgress });
            DataTable dtProgress = dsProgress.Tables[0];

            if (dtProgress.Rows.Count == 0) return;
            DataRow rowProgress = dtProgress.Rows[0];

            _HEADER.CurrentRow["NO_INVOICE_REL"] = noSlipProgress;
            _HEADER.CurrentRow["NM_MENU"] = menuId;
            _HEADER.CurrentRow["FG_REG_TYPE"] = "O"; //INVOICE 생성 관계 구분(O:Order, M: Master, H: House)
            //aTextEdit_ProgressNo.Text = A.GetString(rowProgress["NO_PROGRESS"]);
            //aTextEdit_RefNo.Text = A.GetString(rowProgress["NO_REFERENCE"]);
            aTextEdit_MBLNo.Text = A.GetString(rowProgress["NO_MBL"]);
            //aTextEdit_HBLNo.Text = A.GetString(rowProgress["NO_HBL"]);
            //aTextEdit_Shipper.Text = A.GetString(rowProgress["NM_PARTNER_SHIPPER"]);
            //aTextEdit_Consignee.Text = A.GetString(rowProgress["NM_PARTNER_CONSIGNEE"]);
            //aTextEdit_Notify.Text = A.GetString(rowProgress["NM_PARTNER_NOTIFY"]);
            aDateEdit_PostDate.Text = A.GetString(rowProgress["TM_INVOICE_POST"]);
            
            //2020.10.16 HYJ EU AP 관리 개선 -> INV DATE, DUE DATE 입력 제거 
            if(Global.BizCode == "TYO" || Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
            {
                aDateEdit_InvoiceDate.Text = A.GetString(rowProgress["TM_INVOICE"]);
                aDateEdit_DueDate.Text = A.GetString(rowProgress["TM_INVOICE_DUE"]);
            }

            //aDateEdit_ETD.Text = A.GetString(rowProgress["TM_ETD"]);
            //aDateEdit_ETA.Text = A.GetString(rowProgress["TM_ETA"]);
            //aTextEdit_POL.Text = A.GetString(rowProgress["NM_LOC_POL"]);
            //aTextEdit_POD.Text = A.GetString(rowProgress["NM_LOC_POD"]);
            //aNumericText_PKG.DecimalValue = A.GetDecimal(rowProgress["QT_PACKAGE"]);
            //aTextEdit_PKG.Text = A.GetString(rowProgress["NM_UNIT"]);
            //aNumericText_GWGTK.DecimalValue = A.GetDecimal(rowProgress["QT_GRS_WGT"]);
            //aNumericText_GWGTL.DecimalValue = A.GetDecimal(rowProgress["QT_GRS_WGT1"]);
            //aNumericText_CWGTK.DecimalValue = A.GetDecimal(rowProgress["QT_CHG_WGT"]);
            //aNumericText_CWGTL.DecimalValue = A.GetDecimal(rowProgress["QT_CHG_WGT1"]);
            //aNumericText_CBM.DecimalValue = A.GetDecimal(rowProgress["QT_MEAS"]);
            //aNumericText_CFT.DecimalValue = A.GetDecimal(rowProgress["QT_MEAS1"]);
            //aTextEdit_Incoterms.Text = A.GetString(rowProgress["CD_INCOTERMS"]);
            _HEADER.CurrentRow["NO_SLIP_PROGRESS"] = A.GetString(rowProgress["NO_SLIP_PROGRESS"]);
            _fgShippingMode = A.GetString(rowProgress["FG_SHIPPING_MODE"]);

            if (_ynAcctInfo == "Y")
            {
                DataRow rowOffice = MasterHelper.GetOffice(Global.BizCode);
                DataRow rowPartner = MasterHelper.GetPartner(_cdPartnerBillTo);
                aLookUpEdit_Currency.EditValue = _cdCurrency == string.Empty ? A.GetString(rowOffice["CD_CURRENCY"]) : _cdCurrency;
                aCodeText_Vendor.SetCodeNameNValue(_cdPartnerBillTo, _nmPartnerBillTo);
                aMemoEdit_PaidToAddr.Text = A.GetString(rowPartner["DC_ADDRESS_BL"]);

                DataTable dtFreight = dsProgress.Tables[1];
                DataRow[] temp = dtFreight.Select("FG_FREIGHT = 'AP' AND CD_PARTNER = '" + _cdPartnerBillTo + "' AND CD_CURRENCY = '" + _cdCurrency + "'", "SEQ_FREIGHT");

                SettingFreight(temp);
            }
            else
            {
                DataRow rowOffice = MasterHelper.GetOffice(Global.BizCode);
                aLookUpEdit_Currency.EditValue = A.GetString(rowProgress["CD_CURRENCY"]) == string.Empty ? A.GetString(rowOffice["CD_CURRENCY"]) : A.GetString(rowProgress["CD_CURRENCY"]);
            }

            SettingDueDate();
        }

        private void SettingOfBL(string menuId, string cdBiz, string noSlipBL, string tpBizClass)
        {
            DataTable dtBL = _D.SearchBL(new object[] { Global.FirmCode, cdBiz, noSlipBL, tpBizClass });
            if (dtBL.Rows.Count == 0) return;

            DataRow rowBL = dtBL.Rows[0];
            _HEADER.CurrentRow["NO_INVOICE_REL"] = noSlipBL;
            _HEADER.CurrentRow["NM_MENU"] = menuId;
            _HEADER.CurrentRow["FG_REG_TYPE"] = tpBizClass; //INVOICE 생성 관계 구분(O:Order, M: Master, H: House)
            //aTextEdit_ProgressNo.Text = A.GetString(rowBL["NO_PROGRESS"]);
            //aTextEdit_RefNo.Text = A.GetString(rowBL["NO_REFERENCE"]);
            aTextEdit_MBLNo.Text = A.GetString(rowBL["NO_MBL"]);
            //aTextEdit_HBLNo.Text = A.GetString(rowBL["NO_HBL"]);
            //aCodeText_Vendor.SetCodeNameNValue(A.GetString(rowBL[""]), A.GetString(rowBL[""]));
            //aTextEdit_Shipper.Text = A.GetString(rowBL["NM_PARTNER_SHIPPER"]);
            //aTextEdit_Consignee.Text = A.GetString(rowBL["NM_PARTNER_CONSIGNEE"]);
            //aTextEdit_Notify.Text = A.GetString(rowBL["NM_PARTNER_NOTIFY"]);
            aDateEdit_PostDate.Text = A.GetString(rowBL["TM_INVOICE_POST"]);
            
            //2020.10.16 HYJ EU AP 관리 개선으로 INV DATE, DUE DATE 입력 제거 
            if (Global.BizCode == "TYO" || Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
            {
                aDateEdit_InvoiceDate.Text = A.GetString(rowBL["TM_INVOICE"]);
                aDateEdit_DueDate.Text = A.GetString(rowBL["TM_INVOICE_DUE"]);
            }

            //aDateEdit_ETD.Text = A.GetString(rowBL["TM_ETD"]);
            //aDateEdit_ETA.Text = A.GetString(rowBL["TM_ETA"]);
            //aTextEdit_POL.Text = A.GetString(rowBL["NM_LOC_POL"]);
            //aTextEdit_POD.Text = A.GetString(rowBL["NM_LOC_POD"]);
            //aNumericText_PKG.DecimalValue = A.GetDecimal(rowBL["QT_PACKAGE"]);
            //aTextEdit_PKG.Text = A.GetString(rowBL["NM_UNIT"]);
            //aNumericText_GWGTK.DecimalValue = A.GetDecimal(rowBL["QT_GRS_WGT"]);
            //aNumericText_GWGTL.DecimalValue = A.GetDecimal(rowBL["QT_GRS_WGT1"]);
            //aNumericText_CWGTK.DecimalValue = A.GetDecimal(rowBL["QT_CHG_WGT"]);
            //aNumericText_CWGTL.DecimalValue = A.GetDecimal(rowBL["QT_CHG_WGT1"]);
            //aNumericText_CBM.DecimalValue = A.GetDecimal(rowBL["QT_MEAS"]);
            //aNumericText_CFT.DecimalValue = A.GetDecimal(rowBL["QT_MEAS1"]);
            //aTextEdit_Incoterms.Text = A.GetString(rowBL["CD_INCOTERMS"]);
            _fgShippingMode = A.GetString(rowBL["FG_SHIPPING_MODE"]);

            if (_ynAcctInfo == "Y")
            {
                DataTable dtOrderAcctInfo = _D.SearchOrderAcctInfo(cdBiz, noSlipBL, "AP");

                DataRow rowOffice = MasterHelper.GetOffice(Global.BizCode);
                DataRow rowPartner = MasterHelper.GetPartner(A.GetString(dtOrderAcctInfo.Rows[0]["CD_PARTNER"]));
                aLookUpEdit_Currency.EditValue = A.GetString(dtOrderAcctInfo.Rows[0]["CD_CURRENCY"]) == string.Empty ? A.GetString(rowOffice["CD_CURRENCY"]) : A.GetString(dtOrderAcctInfo.Rows[0]["CD_CURRENCY"]);
                aCodeText_Vendor.SetCodeNameNValue(A.GetString(rowPartner["CD_PARTNER"]), A.GetString(rowPartner["NM_PARTNER_ENG"]));
                aMemoEdit_PaidToAddr.Text = A.GetString(rowPartner["DC_ADDRESS_ACCT"]);

                DataTable dtFreight = dtOrderAcctInfo;
                DataRow[] temp = dtFreight.Select("FG_FREIGHT = 'AP' AND CD_PARTNER = '" + A.GetString(dtOrderAcctInfo.Rows[0]["CD_PARTNER"]) + "' AND CD_CURRENCY = '" + A.GetString(dtOrderAcctInfo.Rows[0]["CD_CURRENCY"]) + "'", "SEQ_FREIGHT");

                SettingFreight(temp);
            }
            else
            {
                DataRow rowOffice = MasterHelper.GetOffice(Global.BizCode);
                aLookUpEdit_Currency.EditValue = A.GetString(rowBL["CD_CURRENCY"]) == string.Empty ? A.GetString(rowOffice["CD_CURRENCY"]) : A.GetString(rowBL["CD_CURRENCY"]);
            }

            _HEADER.CurrentRow["NO_SLIP_PROGRESS"] = A.GetString(rowBL["NO_SLIP_PROGRESS"]);

            SettingDueDate();
        }

        private void SettingFreight(DataRow[] dtFreight)
        {
            for (int i = 0; i < dtFreight.Length; i++)
            {
                Button_ADD_Click(null, null);
                DataRow newRow = bandedGridView1.GetDataRow(bandedGridView1.FocusedRowHandle);
                //newRow["SEQ_FREIGHT"] = dtFreight[i]["SEQ_FREIGHT"];
                newRow["CD_FREIGHT"] = dtFreight[i]["CD_FREIGHT"];
                newRow["NM_FREIGHT"] = dtFreight[i]["NM_FREIGHT"];
                newRow["FG_CALC"] = dtFreight[i]["FG_CALC"];
                newRow["QT_UNIT"] = dtFreight[i]["QT_UNIT"];
                newRow["RT_UNIT"] = dtFreight[i]["RT_UNIT"];
                newRow["CD_CURRENCY"] = dtFreight[i]["CD_CURRENCY"];
                newRow["RT_XCRT"] = dtFreight[i]["RT_XCRT"];
                newRow["RT_FREIGHT_VAT"] = dtFreight[i]["RT_FREIGHT_VAT"];
                newRow["AM_FREIGHT_COST"] = dtFreight[i]["AM_FREIGHT_COST"];
                newRow["AM_FREIGHT_VAT_COST"] = dtFreight[i]["AM_FREIGHT_VAT_COST"];
                newRow["AM_FREIGHT_SUM_COST"] = dtFreight[i]["AM_FREIGHT_SUM_COST"];
                newRow["NO_SLIP_PROGRESS_REL"] = dtFreight[i]["NO_SLIP_PROGRESS"];
                newRow["SEQ_PROGRESS_ACCT_INFO_REL"] = dtFreight[i]["SEQ_FREIGHT"];
                newRow["FG_FREIGHT_REL"] = dtFreight[i]["FG_FREIGHT"];

                //bandedGridView1.FocusedColumn = bandedGridView1.Columns["RT_UNIT"];
                //bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], dtFreight[i]["RT_UNIT"]);
            }

            bandedGridView1.UpdateCurrentRow();
        }

        private void SettingFreight_jp(string gubun, string[] freight, string exRate)
        {
            string cdFreight = string.Empty;
            string nmFreight = string.Empty;

            bandedGridView1.AddNewRow();
            bandedGridView1.UpdateCurrentRow();

            if (freight[0] == "T.H.C." || freight.Contains("JPTHC") || freight[0].Contains("LOADING") || freight[0].Contains("THC"))
            {
                cdFreight = "THC";
                nmFreight = "THC";
                bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["FG_CALC"], "CNTR");
            }
            else if (freight[0].Contains("DOCUMENT") || freight[0] == "JPDOC" || freight[0] == "DOCUMENTATION")
            {
                cdFreight = "DOC-OE";
                nmFreight = "DOCUMENT FEE";
                bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["FG_CALC"], "BL");
            }
            else if (freight[0].Contains("OCEAN") || freight[0] == "OF")
            {
                cdFreight = "OF-OE";
                nmFreight = "OCEAN FREIGHT";
                bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["FG_CALC"], "RT");
            }
            else if (freight[0].Contains("Seal") || freight[0].Contains("SEAL") || freight[0].Contains("JPSF"))
            {
                cdFreight = "CONSEAL";
                nmFreight = "CONTAINER SEAL FEE";
                bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["FG_CALC"], "CNTR");
            }
            else if (freight[0].Contains("AFS"))
            {
                cdFreight = "AFS";
                nmFreight = "ADVANCE FILLING SURCHARGE";
                bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["FG_CALC"], "BL");
            }
            else
            {
                cdFreight = "OTH-OE";
                nmFreight = "OTHER CHARGE";
                bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["FG_CALC"], "BL");
            }

            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["CD_FREIGHT"], cdFreight);
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["NM_FREIGHT"], nmFreight);
            
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_FREIGHT_VAT"], 0);
            bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_VAT_COST"], 0);


            gubun.Split('/');
            switch (gubun)
            {
                case "kmtc_vendor_invoice":

                    if (freight.Length == 8)
                    {
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["QT_UNIT"], freight[1]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], freight[2]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_XCRT"], freight[6]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], freight[7]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], freight[7]);
                    }
                    else if (freight.Length == 9)
                    {
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["QT_UNIT"], freight[2]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], freight[3]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_XCRT"], freight[7]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], freight[8]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], freight[8]);
                    }
                    else if (freight.Length == 10)
                    {
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["QT_UNIT"], freight[3]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], freight[4]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_XCRT"], freight[8]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], freight[9]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], freight[9]);
                    }

                    break;

                case "sitc_vendor_invoice":

                    if (freight[4].Contains("JPY"))
                    {
                        exRate = "1";
                    }

                    if (freight.Length == 8)
                    {
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["QT_UNIT"], freight[1]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], freight[4].Substring(3));
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_XCRT"], exRate);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], freight[7].Substring(3));
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], freight[7].Substring(3));
                    }

                    break;

                case "heung_vendor_invoice":
                case "sinokor_vendor_invoice":

                    string[] value = freight[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    if (value.Length == 7)
                    {
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["QT_UNIT"], 1);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], value[1]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_XCRT"], value[2]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], value[6]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], value[6]);
                    }

                    break;

                case "namsung_vendor_invoice":
                case "pegasus_vendor_invoice":

                    if (freight.Length == 7)
                    {
                        if (freight[5].Contains("JPY"))
                        {
                            exRate = "1";
                        }

                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["QT_UNIT"], freight[2]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], freight[3]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_XCRT"], exRate);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], freight[6]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], freight[6]);
                    }
                    else
                    {
                        if (freight[7].Contains("JPY"))
                        {
                            exRate = "1";
                        }

                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["QT_UNIT"], freight[4]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_UNIT"], freight[5]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["RT_XCRT"], exRate);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_COST"], freight[8]);
                        bandedGridView1.SetRowCellValue(bandedGridView1.FocusedRowHandle, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"], freight[8]);
                    }

                    break;
            }            

            bandedGridView1.UpdateCurrentRow();
        }

        private void SettingAPRequest(string cdBiz, string strNoSlipInvoiceAP)
        {
            if (strNoSlipInvoiceAP != string.Empty)
            {
                DataTable dt = _D.SearchAPRequest(cdBiz, strNoSlipInvoiceAP);
                aTextEdit_NM_STATUS.Text = A.GetString(dt.Rows[0]["NM_STATUS"]);
                aTextEdit_RequestNo.Text = A.GetString(dt.Rows[0]["NO_INVOICE_AP"]);
                _noInvoiceAp = A.GetString(dt.Rows[0]["NO_INVOICE_AP"]);

                //2020.10.16 HYJ - PR 후에 INV DATE, DUE DATE 화면에 세팅
                aDateEdit_InvoiceDate.Text = A.GetString(dt.Rows[0]["TM_INVOICE"]);
                aDateEdit_DueDate.Text = A.GetString(dt.Rows[0]["TM_INVOICE_DUE"]);
                _HEADER.AcceptChanges();
            }
        }

        private void SettingDueDate()
        {
            try
            {
                if (aCodeText_Vendor.GetCodeValue() == string.Empty) return;

                if (_tpBound == "I")
                {
                    aDateEdit_DueDate.Text = aDateEdit_InvoiceDate.Text;
                }
                else
                {
                    DateTime dt = new DateTime();
                    if (!DateTime.TryParse(A.GetString(aDateEdit_InvoiceDate.EditValue), out dt))
                    {
                        return;
                    }

                    /*
                      * 2017.03.04 - KJH
                      * 변경한 PARTNER CODE로 CREDIT TERM을 조회해서 DUE DATE를 수정하는 로직
                      */
                    aDateEdit_DueDate.Text = A.GetString(DBHelper.ExecuteScalar("SELECT dbo.FN_MAS_COLSCHEDULE('" + Global.FirmCode + "','" + Global.BizCode + "','" + aCodeText_Vendor.GetCodeValue() + "','" + A.GetString(aDateEdit_InvoiceDate.Text) + "')"));
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private void OnSavePaymentRequest()
        {
            string tmInvoice = string.Empty;
            string tmInvoiceDue = string.Empty;
            //채번
            string noSlipInvoiceAp = A.GetSlipNoNew(Global.BizCode, "INT", 8);
            _HEADER.CurrentRow["NO_SLIP_INVOICE_AP"] = noSlipInvoiceAp;

            //18.07.10 - KJH 구분 가능한 REQUEST NO.도 채번 할 것 (NO_INVOICE_AP)
            string noInvoiceAp = A.GetSlipNoNew(Global.BizCode, "INT", 15);

            //2020.10.16 HYJ - Invoice Date, Due Date 입력창 추가
            POPUP_PAYMENT_REQUEST pop = new POPUP_PAYMENT_REQUEST(aCodeText_Vendor.CodeValue, aDateEdit_InvoiceDate.Text, aDateEdit_DueDate.Text);

            if (pop.ShowDialog() == DialogResult.OK)
            {
                string[] rtn = A.GetString(pop.ReturnData["ReturnData"]).Split('/');

                if (rtn == null) return;

                tmInvoice = rtn[0];
                tmInvoiceDue = rtn[1];
            }
            else
            {
                throw new Exception("You canceled it.");
            }

            object[] obj = new object[]
            {
                Global.FirmCode,
                _HEADER.CurrentRow["CD_BIZ"],
                noSlipInvoiceAp,
                noInvoiceAp,
                _noSlipInvoice,
                Global.UserID,
                "REQUEST",
                tmInvoice,
                tmInvoiceDue
            };

            if (_D.SaveInvoiceAp(obj))
            {
                //MANAGER에게 REQUEST 메일 발송
                POPUP_MAIL_SEND POPUP_MAIL_SEND = new POPUP_MAIL_SEND();
                POPUP_MAIL_SEND.txtMailFrom = "aims1@aifcompany.com";
                POPUP_MAIL_SEND.Password = "bsn@123";
                POPUP_MAIL_SEND.txtMailCc = "";
                //POPUP_MAIL_SEND.txtMailBCc = "jinhyuk@aifcompany.com";
                POPUP_MAIL_SEND.txtMailSubject = "[PAYMENT REQUEST] " + _HEADER.CurrentRow["NM_PARTNER_BILL_TO"] + " (" + DateTime.Now.ToString("MM/dd/yyyy") + ", Requested by - " + Global.EmpEnName + ")";
                POPUP_MAIL_SEND.txtContent = GetContents();
                POPUP_MAIL_SEND.txtMailTo = GetMailTo();
                POPUP_MAIL_SEND.mailFont = "맑은 고딕";
                POPUP_MAIL_SEND.mailFontSize = "10pt";
                POPUP_MAIL_SEND.FileAttach = null;
                POPUP_MAIL_SEND.InnerImageAttach = null;
                POPUP_MAIL_SEND.SendMail();

                ShowMessageBoxA("Requested successfully!", MessageType.Information);

                SettingAPRequest(_HEADER.CurrentRow["CD_BIZ"].ToString(), noSlipInvoiceAp);
            }
        }

        private void DataValidCheck()
        {
            string amVender = A.GetString(bandedGridView1.GetRowCellValue(0, bandedGridView1.Columns["AM_FREIGHT_SUM_COST"]));

            if (aTextEdit_InvoiceNo.Text != string.Empty
                && aDateEdit_DueDate.Text != string.Empty
                //&& aDateEdit_TM_INVOICE_RECEIVED.Text != string.Empty
                && amVender != string.Empty)
            {
                aLabel_DataNo.Visible = false;
                aLabel_DataYes.Visible = true;
            }
            else
            {
                aLabel_DataNo.Visible = true;
                aLabel_DataYes.Visible = false;
            }

            amVender = aNumericText_TotalAmount.Text;
            aNumericText_AfterProfit.Text = A.GetString(Double.Parse(aNumericText_Profit.Text) - Double.Parse(amVender == string.Empty ? "0.00" : amVender));

        }

        private void TariffCalculate()
        {
            try
            {
                POPUP_TARIFF pop = new POPUP_TARIFF();

                pop.SetPartnerCode = aCodeText_Vendor.CodeValue;
                pop.SetPartnerName = aCodeText_Vendor.CodeName;

                pop.SetShipperCode = A.GetString(_HEADER.CurrentRow["CD_PARTNER_SHIPPER"]);
                //pop.SetShipperName = aTextEdit_Shipper.Text;
                pop.SetConsigneeCode = A.GetString(_HEADER.CurrentRow["CD_PARTNER_CONSIGNEE"]);
                //pop.SetConsigneeName = aTextEdit_Consignee.Text;
                pop.SetPOLCode = A.GetString(_HEADER.CurrentRow["CD_LOC_POL"]);
                //pop.SetPOLName = aTextEdit_POL.Text;
                pop.SetPODCode = A.GetString(_HEADER.CurrentRow["CD_LOC_POD"]);
                //pop.SetPODName = aTextEdit_POD.Text;
                //pop.SetIncoterms = A.GetString(aTextEdit_Incoterms.EditValue);
                pop.SetTransMode = _tpAirSea == "A" ? "AIR" : _fgShippingMode;

                if (pop.ShowDialog() == DialogResult.OK)
                {
                    DataTable dtTariff = (DataTable)pop.ReturnData["ReturnDataTable"];
                    DataRow[] drsAP = dtTariff.Select("FG_INVOICE = 'AP'");
                    DataTable dtFreight = CH.GetCode("MAS_FREIGHT", new string[1] { "XX" });
                    dtFreight.PrimaryKey = new DataColumn[] { dtFreight.Columns["CODE"] };

                    foreach (DataRow row in drsAP)
                    {
                        decimal qtStart = A.GetDecimal(row["QT_START"]);
                        decimal qtEnd = A.GetDecimal(row["QT_END"]);
                        string fgCalc = A.GetString(row["FG_CALC"]);

                        // RANGE 존재하는지 체크, 존재하면 해당 RANGE에 무게가 포함되는지 체크, 포함되지 않으면 PASS
                        if (qtStart != 0 || qtEnd != 0)
                        {
                            decimal qtCompare = 0;

                            //if (fgCalc == "CWGT")
                            //{
                            //    qtCompare = aNumericText_CWGTK.DecimalValue;
                            //}
                            //else if (fgCalc == "GWGT")
                            //{
                            //    qtCompare = aNumericText_GWGTK.DecimalValue;
                            //}

                            if (qtCompare < qtStart || qtCompare >= qtEnd)
                            {
                                continue;
                            }
                        }

                        Button_ADD_Click(null, null);
                        DataRow newRow = bandedGridView1.GetDataRow(bandedGridView1.FocusedRowHandle);
                        newRow["CD_FREIGHT"] = row["CD_FREIGHT"];
                        newRow["NM_FREIGHT"] = row["NM_FREIGHT"];
                        newRow["FG_CALC"] = fgCalc;
                        newRow["QT_UNIT"] = GetQtUnit(fgCalc);

                        decimal minimum = 0m;

                        if (A.GetString(row["FG_NET"]) == "N")          //NET
                        {
                            minimum = A.GetDecimal(row["AM_MIN"]);
                            newRow["RT_UNIT"] = row["RT_FREIGHT_NET"];
                        }
                        else if (A.GetString(row["FG_NET"]) == "NS")    //NET-NET
                        {
                            minimum = A.GetDecimal(row["QT_MIN_SELL"]);
                            newRow["RT_UNIT"] = row["RT_FREIGHT_SELL"];
                        }

                        if (A.GetDecimal(newRow["QT_UNIT"]) * A.GetDecimal(newRow["RT_UNIT"]) < minimum)
                            newRow["AM_FREIGHT_COST"] = minimum;
                        else
                            newRow["AM_FREIGHT_COST"] = A.GetDecimal(newRow["QT_UNIT"]) * A.GetDecimal(newRow["RT_UNIT"]);

                        DataRow rowFreight = dtFreight.Rows.Find(newRow["CD_FREIGHT"]);

                        if (rowFreight != null)
                        {
                            newRow["RT_FREIGHT_VAT"] = A.GetDecimal(rowFreight["NAME1"]);
                            newRow["AM_FREIGHT_VAT_COST"] = A.GetDecimal(newRow["AM_FREIGHT_COST"]) * A.GetDecimal(newRow["RT_FREIGHT_VAT"]) / 100m;
                        }

                        newRow["CD_CURRENCY"] = row["CD_CURRENCY"];

                        int floatPoint = 0;
                        if (A.GetString(newRow["CD_CURRENCY"]) != "KRW" && A.GetString(newRow["CD_CURRENCY"]) != "JPY") floatPoint = 2;

                        newRow["AM_FREIGHT_COST"] = decimal.Round(A.GetDecimal(newRow["AM_FREIGHT_COST"]), floatPoint, MidpointRounding.AwayFromZero);
                        newRow["AM_FREIGHT_VAT_COST"] = decimal.Round(A.GetDecimal(newRow["AM_FREIGHT_VAT_COST"]), floatPoint, MidpointRounding.AwayFromZero);
                        newRow["AM_FREIGHT_SUM_COST"] = A.GetDecimal(newRow["AM_FREIGHT_COST"]) + A.GetDecimal(newRow["AM_FREIGHT_VAT_COST"]);
                    }

                    bandedGridView1.UpdateCurrentRow();
                }
            }
            catch (Exception ex)
            {
                HandleWinException(ex);
            }
        }

        private decimal GetQtUnit(string fgCalc)
        {
            decimal qtUnit = 1m;

            //if (fgCalc == "BL")
            //    qtUnit = 1m;
            //else if (fgCalc == "CBM")
            //    qtUnit = aNumericText_CBM.DecimalValue < 1m ? 1m : aNumericText_CBM.DecimalValue;
            //else if (fgCalc == "CWGT")
            //    qtUnit = aNumericText_CWGTK.DecimalValue;
            //else if (fgCalc == "GWGT")
            //    qtUnit = aNumericText_GWGTK.DecimalValue;
            //else if (fgCalc == "PCS")
            //    qtUnit = aNumericText_PKG.DecimalValue;
            //else if (fgCalc == "RT")
            //    qtUnit = CalcRevenueTon();
            //else if (fgCalc == "TON")
            //{
            //    qtUnit = decimal.Round(aNumericText_GWGTK.DecimalValue / 1000m, 2, MidpointRounding.AwayFromZero);
            //    qtUnit = qtUnit < 1m ? 1m : qtUnit;
            //}

            return qtUnit;
        }

        private decimal CalcRevenueTon()
        {
            //decimal GWeight = aNumericText_GWGTK.DecimalValue / 1000m;
            //decimal measurement = aNumericText_CBM.DecimalValue;
            decimal revenueTon = 1m;

            // Revenue Ton의 최소값 : 1
            //if (GWeight == decimal.Zero && measurement == decimal.Zero)
            //    revenueTon = decimal.Zero;
            //else if (GWeight > measurement)
            //    revenueTon = GWeight > 1m ? GWeight : 1m;
            //else
            //    revenueTon = measurement > 1m ? measurement : 1m;

            return revenueTon;
        }

        private string SettingNoInvoiceAp(string noInvoiceRel, string cdPartner)
        {
            //인보이스 생성 이력 있는지 확인
            DataTable dt = _D.SearchAPInvoice(noInvoiceRel, cdPartner);

            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["NO_SLIP_INVOICE"].ToString();
            }
            else
            {
                return null;
            }
        }

        private bool BeforeSave()
        {
            if(_noSlipInvoice != string.Empty)
            {
                ShowMessageBoxA("You can only create the first time.", MessageType.Warning);
                return false;
            }

            if(_pdfFilePath == string.Empty)
            {
                ShowMessageBoxA("You must have a pdf file to attach.", MessageType.Warning);
                return false;
            }

            if (bandedGridView1.RowCount == 0)
            {
                ShowMessageBoxA("You can save when there is more than one line data.", MessageType.Warning);
                return false;
            }

            if (aCodeText_Vendor.CodeValue == string.Empty)
            {
                ShowMessageBoxA(aLabel_PaidTo.Text + " is a required input fields.", MessageType.Warning);
                aCodeText_Vendor.Focus();
                return false;
            }

            if (A.GetString(aLookUpEdit_Currency.EditValue) == string.Empty)
            {
                ShowMessageBoxA(aLabel_Currency.Text + " is default item for save.\r\nPlease select " + aLabel_Currency.Text, MessageType.Warning);
                aLookUpEdit_Currency.Focus();
                return false;
            }

            //2020.10.16 HYJ EU AP 관리 개선 > INV DATE, DUE DATE 입력 제거 
            if (Global.BizCode == "TYO" || Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
            {
                if (A.GetString(aDateEdit_InvoiceDate.Text) == string.Empty)
                {
                    ShowMessageBoxA(aLabel_InvoiceDate.Text + " is default item for save.\r\nPlease select " + aLabel_InvoiceDate.Text, MessageType.Warning);
                    aDateEdit_InvoiceDate.Focus();
                    return false;
                }
            }

            if (!CheckDUE())
            {
                aDateEdit_DueDate.Select();
                return false;
            }

            //ORDER, B/L과의 관계없이 저장되는 현상을 방지하기 위해 POST DATE 체크로직을 검(2017.01.10)
            if (aDateEdit_PostDate.Text == string.Empty)
            {
                ShowMessageBoxA(aLabel_PostDate.Text + " is a required input fields.", MessageType.Warning);
                aDateEdit_PostDate.Select();
                return false;
            }

            if (Global.BizCode == "MIL")
            {
                if (A.GetDecimal(bandedGridView1.Columns["AM_FREIGHT_SUM_COST"].SummaryItem.SummaryValue) == decimal.Zero)
                {
                    ShowMessageBoxA("[Total Amount] must be greater than 0.", MessageType.Warning);
                    return false;
                }
            }

            string[] VerifyNotNull = new string[] { "CD_FREIGHT", "TM_INVOICE_POST" };

            if (!CheckColumn(aGrid_Freight, VerifyNotNull)) return false;

            return true;
        }

        private bool CheckDUE()
        {
            if (aDateEdit_DueDate.Text == string.Empty) return true;

            if (aDateEdit_InvoiceDate.DateTime > aDateEdit_DueDate.DateTime)
            {
                ShowMessageBoxA("Due date should be later than Invoice date.", MessageType.Warning);
                return false;
            }

            return true;
        }

        private bool CheckColumn(aGrid grid, string[] verifyNotNull)
        {
            DataTable dt = grid.DataSource as DataTable;
            DataTable dtChanges = dt.GetChanges();

            if (dtChanges == null) return true;

            foreach (DataRow row in dtChanges.Rows)
            {
                if (row.RowState == DataRowState.Deleted) continue;

                foreach (string item in verifyNotNull)
                {
                    if (dtChanges.Columns[item].DataType != typeof(decimal))
                    {
                        if (A.GetString(row[item]) == string.Empty)
                        {
                            ShowMessageBoxA("'" + bandedGridView1.Columns[item].Caption + "' is a required input fields.", MessageType.Warning);
                            return false;
                        }
                    }

                    if (dtChanges.Columns[item].DataType == typeof(decimal))
                    {
                        if (A.GetDecimal(row[item]) == decimal.Zero)
                        {
                            ShowMessageBoxA("'" + bandedGridView1.Columns[item].Caption + "' is a required input fields.", MessageType.Warning);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool OnSaveFile()
        {
            bool uploadResult = false;
            string filename = Path.GetFileName(aTextEdit_filename.Text);
            string ext = Path.GetExtension(aTextEdit_filename.Text).Replace(".", "");
            string filePath = Global.BizCode + "/" + Global.UserID + "/" + _noSlipInvoice;
            FileInfo fi = new FileInfo(aTextEdit_filename.Text);

            if (fi.Exists)
            {
                uploadResult = _ftpUtil.Upload("INV", filePath, aTextEdit_filename.Text, filename);

                if (uploadResult)
                {
                    object[] obj = new object[]
                    {
                        Global.FirmCode,
                        Global.BizCode,
                        "INV",
                        _noSlipInvoice,
                        1,
                        "",
                        "",
                        filePath,
                        filename,
                        ext,
                        DateTime.Now.ToString("yyyyMMdd"),
                        DateTime.Now.ToString("HHmm"),
                        fi.Length,
                        "",
                        Global.UserID
                    };

                    if (_D.SaveFile(obj))
                    {
                        aTextEdit_AttachInvFile.Text = filename;
                    }
                }
                else
                {
                    //파일 업로드 실패?!
                }
            }

            return uploadResult;
        }

        private DataTable GetdtContainer()
        {
            string cdBiz = A.GetString(_HEADER.CurrentRow["CD_BIZ"]) == string.Empty ? Global.BizCode : A.GetString(_HEADER.CurrentRow["CD_BIZ"]);
            string fgRegType = A.GetString(_HEADER.CurrentRow["FG_REG_TYPE"]);
            string noSlioRel = A.GetString(_HEADER.CurrentRow["NO_INVOICE_REL"]);

            DataTable dtContainer = _D.SearchContainer(new object[] { Global.FirmCode, cdBiz, noSlioRel, fgRegType });

            return dtContainer;
        }

        private string GetContents()
        {
            string result = string.Empty;

            result += "[" + _HEADER.CurrentRow["NM_PARTNER_BILL_TO"] + "] will give you a payment request." + "</br>";
            result += "Portal No. : " + _HEADER.CurrentRow["NO_PROGRESS"] + "</br>";
            result += "Total Amount : " + aNumericText_TotalAmount.Text + "</br>";
            result += "After review, please approve payment." + "</br></br>";

            if (aMemoEdit1.Text != string.Empty)
            {
                result += "Please refer to the following for specific information." + "</br>";
                result += aMemoEdit1.Text.Replace("\r\n", "</br>");
            }

            return result;
        }

        private string GetMailTo()
        {
            string result = string.Empty;
            string cdBiz = _HEADER.CurrentRow["CD_BIZ"].ToString();

            if (cdBiz == "FRA")
            {
                result = "davidcho@atlanticif.com";
            }
            else if (cdBiz == "HAM")
            {
                result = "seanham@atlanticif.com";
            }
            else if (cdBiz == "PAR")
            {
                result = "celinekim@atlanticif.com";
            }
            else if (cdBiz == "MIL")
            {
                result = "charles.kim@atlanticif.com";
            }
            else if (cdBiz == "TYO")
            {
                result = "hjcho@aifcompany.com";
            }
            else if (cdBiz == "LAX" || cdBiz == "CHI" || cdBiz == "NYC")
            {
                result = "ac@allstateif.com";
                //result = "VANNIE.LEE@AIFCOMPANY.COM";
            }

            return result;
        }

        #endregion

    }
}
