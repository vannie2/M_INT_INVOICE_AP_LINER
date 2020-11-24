using NF.A2P;
using System.Collections.Generic;
using System.Data;
using System;
using NF.A2P.Helper;

namespace INT
{
    class M_INT_INVOICE_AP_LINER_D
    {
        internal DataSet Search(object[] obj)
        {
            DataSet ds = DBHelper.GetDataSet("AP_INT_INVOICE_LINER_S", obj);
            return ds;
        }

        internal DataSet Search2(object[] obj)
        {
            if (Global.BizCode == "LAX" || Global.BizCode == "CHI" || Global.BizCode == "NYC")
            {
                DataSet ds = DBHelper.GetDataSet("AP_INT_INVOICE_LINER_S2_US", obj);
                return ds;
            }
            else
            {
                DataSet ds = DBHelper.GetDataSet("AP_INT_INVOICE_LINER_S2", obj);
                return ds;
            }
        }

        internal bool Save(DataTable dtInvoice, DataTable dtFreight, List<string> list)
        {
            SpInfoCollection sc = new SpInfoCollection();

            if (dtInvoice != null)
            {
                SpInfo si = new SpInfo();
                si.DataValue = dtInvoice;
                si.FirmCode = Global.FirmCode;
                si.UserID = Global.UserID;
                si.SpNameInsert = "AP_INT_INVOICE_I";
                si.SpNameUpdate = "AP_INT_INVOICE_I";
                si.SpParamsInsert = new string[] { "CD_FIRM", "CD_BIZ", "NO_SLIP_INVOICE", "FG_INVOICE", "NO_INVOICE",
                                                   "NM_MENU", "FG_REG_TYPE", "NO_INVOICE_REL", "NO_SLIP_INVOICE_AP", "CD_INVOICE_STATUS",
                                                   "CD_PARTNER_BILL_TO", "NM_PARTNER_BILL_TO", "DC_PARTNER_BILL_TO_ADDR", "CD_PARTNER_SHIP_TO", "NM_PARTNER_SHIP_TO",
                                                   "DC_PARTNER_SHIP_TO_ADDR", "DC_RMK", "DC_ATTN_TO", "CD_CURRENCY", "TM_INVOICE",
                                                   "TM_INVOICE_DUE", "TM_INVOICE_POST", "TM_PAY_LAST", "TM_INVOICE_RECEIVED",
                                                   "CD_CURRENCY_STANDARD", "RT_XCRT_STANDARD", "AM_INVOICE_STANDARD", "YN_INVOICE_RECEIVED",
                                                   "CD_USER_REG" };
                si.SpParamsUpdate = new string[] { "CD_FIRM", "CD_BIZ", "NO_SLIP_INVOICE", "FG_INVOICE", "NO_INVOICE",
                                                   "NM_MENU", "FG_REG_TYPE", "NO_INVOICE_REL", "NO_SLIP_INVOICE_AP", "CD_INVOICE_STATUS",
                                                   "CD_PARTNER_BILL_TO", "NM_PARTNER_BILL_TO", "DC_PARTNER_BILL_TO_ADDR", "CD_PARTNER_SHIP_TO", "NM_PARTNER_SHIP_TO",
                                                   "DC_PARTNER_SHIP_TO_ADDR", "DC_RMK", "DC_ATTN_TO", "CD_CURRENCY", "TM_INVOICE",
                                                   "TM_INVOICE_DUE", "TM_INVOICE_POST", "TM_PAY_LAST", "TM_INVOICE_RECEIVED",
                                                   "CD_CURRENCY_STANDARD", "RT_XCRT_STANDARD", "AM_INVOICE_STANDARD", "YN_INVOICE_RECEIVED",
                                                   "CD_USER_REG" };
                sc.Add(si);
            }

            if (dtFreight != null)
            {
                SpInfo si = new SpInfo();
                si.DataValue = dtFreight;
                si.FirmCode = Global.FirmCode;
                si.UserID = Global.UserID;
                si.SpNameInsert = "AP_INT_FREIGHT_I";
                si.SpNameUpdate = "AP_INT_FREIGHT_U";
                si.SpNameDelete = "AP_INT_FREIGHT_D";
                si.SpParamsInsert = new string[] { "CD_FIRM", "CD_BIZ1", "NO_SLIP_INVOICE1", "SEQ_FREIGHT", "CD_FREIGHT", "NM_FREIGHT", "FG_REV_COST", "FG_FREIGHT_TERM", "CD_CURRENCY_INVOICE1", "CD_CURRENCY",
                                                   "RT_XCRT", "FG_CALC", "QT_UNIT", "RT_UNIT", "RT_FREIGHT_VAT", "AM_FREIGHT", "AM_FREIGHT_VAT", "AM_FREIGHT_SUM", "AM_FREIGHT_COST", "AM_FREIGHT_VAT_COST",
                                                   "AM_FREIGHT_SUM_COST", "NO_SLIP_BL1", "TM_INVOICE_POST", "CD_USER_REG", "NO_SLIP_PROGRESS_REL", "FG_FREIGHT_REL", "SEQ_PROGRESS_ACCT_INFO_REL" };
                si.SpParamsUpdate = new string[] { "CD_FIRM", "CD_BIZ1", "NO_SLIP_INVOICE1", "SEQ_FREIGHT", "CD_FREIGHT", "NM_FREIGHT", "FG_REV_COST", "FG_FREIGHT_TERM", "CD_CURRENCY_INVOICE1", "CD_CURRENCY",
                                                   "RT_XCRT", "FG_CALC", "QT_UNIT", "RT_UNIT", "RT_FREIGHT_VAT", "AM_FREIGHT", "AM_FREIGHT_VAT", "AM_FREIGHT_SUM", "AM_FREIGHT_COST", "AM_FREIGHT_VAT_COST",
                                                   "AM_FREIGHT_SUM_COST", "NO_SLIP_BL1", "TM_INVOICE_POST", "CD_USER_AMD", "NO_SLIP_PROGRESS_REL", "FG_FREIGHT_REL", "SEQ_PROGRESS_ACCT_INFO_REL" };
                si.SpParamsDelete = new string[] { "CD_FIRM", "CD_BIZ1", "NO_SLIP_INVOICE1", "SEQ_FREIGHT", "CD_USER_AMD" };
                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Insert, "CD_BIZ1", list[0]);
                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Insert, "NO_SLIP_INVOICE1", list[1]);
                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Insert, "NO_SLIP_BL1", list[2]);
                //si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Insert, "TM_INVOICE_POST1", list[3]);
                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Insert, "CD_CURRENCY_INVOICE1", list[3]);

                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Update, "CD_BIZ1", list[0]);
                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Update, "NO_SLIP_INVOICE1", list[1]);
                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Update, "NO_SLIP_BL1", list[2]);
                //si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Update, "TM_INVOICE_POST1", list[3]);
                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Update, "CD_CURRENCY_INVOICE1", list[3]);

                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Delete, "CD_BIZ1", list[0]);
                si.SpParamsValues.Add(NF.A2P.CommonFunction.SpState.Delete, "NO_SLIP_INVOICE1", list[1]);
                sc.Add(si);
            }

            return DBHelper.Save(sc);
        }

        internal bool Delete(object[] obj)
        {
            return DBHelper.ExecuteNonQuery("AP_INT_INVOICE_D", obj);
        }

        internal DataSet SearchProgress(object[] obj)
        {
            DataSet ds = DBHelper.GetDataSet("AP_INT_PROGRESS_APPLY_S3", obj);
            return ds;
        }

        internal DataTable SearchBL(object[] obj)
        {
            DataTable dt = DBHelper.GetDataTable("AP_INT_BL_APPLY_S", obj);
            return dt;
        }

        internal DataTable SearchAttn(object[] obj)
        {
            string sqlQuery = " SELECT  C.CD_BIZ, C.CD_PARTNER, C.SEQ_PARTNER_CONTACT, C.NM, C.NM_DEPT, " +
                                " C.NM_GRADE, C.NM_DUTY, C.NO_TEL, C.NO_TEL1, C.NO_FAX, C.DC_EMAIL, C.DC_RMK, C.YN_REP, C.YN_EMAIL " +
                                " FROM    MAS_PARTNER_CONTACT C " +
                                " INNER JOIN MAS_PARTNER P ON C.CD_FIRM = P.CD_FIRM AND C.CD_BIZ = P.CD_BIZ AND C.CD_PARTNER = P.CD_PARTNER " +
                                " WHERE   P.CD_FIRM           = '" + obj[0] + "'" +
                                " AND	  C.YN_REP = 'Y'" +
                                " AND     P.CD_PARTNER  = '" + obj[1] + "'";

            DataTable dt = DBHelper.GetDataTable(sqlQuery);
            return dt;
        }

        internal string SearchNoSlipInvoice(string cdBiz, string noSlipProgress, decimal seqAcctInfo)
        {
            string sqlQuery = " SELECT  NO_SLIP_INVOICE" +
                              " FROM    INT_FREIGHT" +
                              " WHERE   CD_FIRM = '" + Global.FirmCode + "'" +
                              " AND     CD_BIZ = '" + cdBiz + "'" +
                              " AND     NO_SLIP_PROGRESS_REL = '" + noSlipProgress + "'" +
                              " AND     SEQ_PROGRESS_ACCT_INFO_REL = " + seqAcctInfo + "" +
                              " AND     FG_FREIGHT_REL = 'AP'";

            DataTable dt = DBHelper.GetDataTable(sqlQuery);

            if (dt == null || dt.Rows.Count == 0) return string.Empty;
            else return A.GetString(dt.Rows[0]["NO_SLIP_INVOICE"]);
        }

        internal DataTable SearchContainer(object[] obj)
        {
            return DBHelper.GetDataTable("AP_INT_INVOICE_CONTAINER_S", obj);
        }

        internal DataTable SearchOrderAcctInfo(string cdBiz, string noSlipBL, string fgFreight)
        {
            return DBHelper.GetDataTable("AP_INT_ORDER_ACCT_INFO_APPLY_S", new object[] { Global.FirmCode, cdBiz, noSlipBL, fgFreight });
        }

        internal DataTable CheckInvoiceNo(string cdBiz, string noSlipInvoice, string noInvoice)
        {
            string sqlQuery = " SELECT  NO_INVOICE" +
                              " FROM    INT_INVOICE" +
                              " WHERE   CD_FIRM = '" + Global.FirmCode + "'" +
                              " AND     CD_BIZ = '" + cdBiz + "'" +
                              " AND     NO_SLIP_INVOICE <> '" + noSlipInvoice + "'" +
                              " AND     NO_INVOICE = '" + noInvoice + "'" +
                              " AND     NO_INVOICE <> ''" +
                              " AND     FG_INVOICE = 'AP'";

            DataTable dt = DBHelper.GetDataTable(sqlQuery);
            return dt;
        }

        internal bool Update(object[] obj)
        {
            return DBHelper.ExecuteNonQuery("AP_INT_INVOICE_AP_U4", obj);
        }

        internal object UpdateClose(string noSlipInvoice, string ynCloseAccount)
        {
            string sqlQuery = " UPDATE  INT_INVOICE " +
                              " SET     YN_CLOSE_ACCOUNT = '" + ynCloseAccount + "'" +
                              " WHERE   CD_FIRM = '" + Global.FirmCode + "'" +
                              //" AND     CD_BIZ = '" + Global.BizCode + "'" +
                              " AND     NO_SLIP_INVOICE = '" + noSlipInvoice + "'" +
                              " AND     FG_INVOICE = 'AP'";

            return DBHelper.ExecuteNonQuery(sqlQuery);
        }

        internal void UpdateInvoicePrint(string cdBiz, string noSlipInvoice)
        {
            string sqlQuery = " UPDATE  INT_INVOICE " +
                              " SET     YN_PRINT = 'Y'" +
                              " WHERE   CD_FIRM = '" + Global.FirmCode + "'" +
                              " AND     CD_BIZ = '" + cdBiz + "'" +
                              " AND     NO_SLIP_INVOICE = '" + noSlipInvoice + "'";

            DBHelper.ExecuteNonQuery(sqlQuery);
        }

        internal bool SaveInvoiceAp(object[] obj)
        {
            return DBHelper.ExecuteNonQuery("AP_INT_INVOICE_AP_I2", obj);
        }

        internal bool SaveFile(object[] obj)
        {
            return DBHelper.ExecuteNonQuery("AP_MAS_FILE_I", obj);
        }

        internal DataTable SearchAPRequest(string cdBiz, string strNoSlipInvoiceAP)
        {
            string sqlQuery = " SELECT  NO_INVOICE_AP, NM_STATUS, TM_INVOICE, TM_INVOICE_DUE" +
                              " FROM    INT_INVOICE_AP" +
                              " WHERE   CD_FIRM = '" + Global.FirmCode + "'" +
                              " AND     CD_BIZ = '" + cdBiz + "'" +
                              " AND     NO_SLIP_INVOICE_AP = '" + strNoSlipInvoiceAP + "'";

            DataTable dt = DBHelper.GetDataTable(sqlQuery);
            return dt;
        }

        internal DataTable SearchAPInvoice(string noInvoiceRel, string cdPartner)
        {
            string sqlQuery = " SELECT NO_SLIP_INVOICE, NO_SLIP_INVOICE_AP" +
                              " FROM   INT_INVOICE" +
                              " WHERE  CD_FIRM = '" + Global.FirmCode + "'" +
                              " AND    CD_BIZ = '" + Global.BizCode + "'" +
                              " AND    NO_INVOICE_REL = '" + noInvoiceRel + "'" +
                              " AND    CD_PARTNER_BILL_TO = '" + cdPartner + "'" +
                              " AND    FG_INVOICE = 'AP'";

            DataTable dt = DBHelper.GetDataTable(sqlQuery);
            return dt;
        }

        internal DataTable SearchFile(object[] obj)
        {
            string query = string.Empty;

            query += " SELECT NM_FILE_PATH, NM_FILE, QT_FILE_SIZE, NM_FILE_EXT "
                  + "  FROM   MAS_FILE "
                  + "  WHERE  CD_FIRM = '" + obj[0] + "'"
                  //+ "  AND    CD_BIZ = '" + obj[1] + "'"               
                  + "  AND    NO_SLIP_REL = '" + obj[1] + "'";

            DataTable dt = DBHelper.GetDataTable(query);
            return dt;
        }

        internal void UpdateInvoiceYn(string cdBiz, string noSlipInvoice)
        {
            string sqlQuery = " UPDATE  INT_INVOICE " +
                              " SET     YN_AUTO = 'Y'" +
                              " WHERE   CD_FIRM = '" + Global.FirmCode + "'" +
                              " AND     CD_BIZ = '" + cdBiz + "'" +
                              " AND     NO_SLIP_INVOICE = '" + noSlipInvoice + "'";

            DBHelper.ExecuteNonQuery(sqlQuery);
        }

        internal void UpdateMbl(string cdBiz, string noSlipBl, string noBl)
        {
            string sqlQuery = " UPDATE  INT_BL " +
                              " SET     NO_BL = '" + noBl + "'" +
                              " WHERE   CD_FIRM = '" + Global.FirmCode + "'" +
                              " AND     CD_BIZ = '" + cdBiz + "'" +
                              " AND     NO_SLIP_BL = '" + noSlipBl + "'";

            DBHelper.ExecuteNonQuery(sqlQuery);
        }
    }
}
