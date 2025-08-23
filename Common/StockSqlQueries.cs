﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class StockSqlQueries
    {
        public static string StockQuery => @"SELECT ([T2_BRA].[REF] + [F7]) AS NewStyle,
			Sum(T2_BRA.Q11) AS QTY1, Sum(T2_BRA.Q12) AS QTY2, 
                Sum(T2_BRA.Q13) AS QTY3, Sum(T2_BRA.Q14) AS QTY4, Sum(T2_BRA.Q15) AS QTY5, Sum(T2_BRA.Q16) AS QTY6, Sum(T2_BRA.Q17) AS QTY7, Sum(T2_BRA.Q18) AS QTY8, 
                    Sum(T2_BRA.Q19) AS QTY9, Sum(T2_BRA.Q20) AS QTY10, Sum(T2_BRA.Q21) AS QTY11, Sum(T2_BRA.Q22) AS QTY12, Sum(T2_BRA.Q23) AS QTY13, T2_HEAD.REF,
                        Sum(T2_BRA.LY11) AS LY1, Sum(T2_BRA.LY12) AS LY2, Sum(T2_BRA.LY13) AS LY3, Sum(T2_BRA.LY14) AS LY4, Sum(T2_BRA.LY15) AS LY5, 
                            Sum(T2_BRA.LY16) AS LY6, Sum(T2_BRA.LY17) AS LY7, Sum(T2_BRA.LY18) AS LY8, Sum(T2_BRA.LY19) AS LY9, Sum(T2_BRA.LY20) AS LY10,
                                Sum(T2_BRA.LY21) AS LY11, Sum(T2_BRA.LY22) AS LY12, Sum(T2_BRA.LY23) AS LY13, T2_HEAD.LASTDELV, T2_HEAD.SELL, T2_HEAD.REM, T2_HEAD.REM2, T2_HEAD.USER1
									FROM ((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF)) INNER JOIN 
									(SELECT Right(t.[KEY],3) AS NewCol, t.F1 AS MasterColour, Left(t.[KEY],3) AS Col, t.F7 
								FROM T2_LOOK t
								WHERE (Left(t.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN [DESC] ON [T2_BRA].[REF] = [DESC].SKU)
                                    WHERE T2_BRA.BRANCH in ('T','G', 'A')
									GROUP BY ([T2_BRA].[REF] + [F7]),T2_HEAD.REF, T2_HEAD.LASTDELV, T2_HEAD.SELL, T2_HEAD.REM, T2_HEAD.REM2, T2_HEAD.USER1";

        public static string GetEanCodes => "SELECT * from T2_EAN WHERE EAN_CODE <> NULL";

        public static string InsertSKU => @"INSERT INTO [DESC] (SKU) VALUES (?)";

        public static string DoesSKUExist => @"SELECT SKU FROM [DESC] WHERE SKU = (?)";

        public static string DeleteSKUs => @"DELETE FROM [DESC]";

        public static string GetSalesValues => @"SELECT * FROM SALES_AUDIT";

        public static string UpdateSalesValues => @"UPDATE [SALES_AUDIT] (LAST_MONTH, LAST_WEEK, YES_SALES) VALUES(?,?,?) where SKU = ?";

        public static string InsertNewProduct => @"INSERT INTO [SALES_AUDIT] (SKU) VALUE(?)";
    }
}
