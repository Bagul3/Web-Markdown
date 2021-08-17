using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class SqlQueries
    {
		public static string StockQueryALL => @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT, T2_HEAD.MINSIZE, T2_HEAD.MAXSIZE,
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF, T2_HEAD.VAT, T2_HEAD.REM, T2_HEAD.REM2, T2_HEAD.BASESELL, T2_HEAD.SELL, T2_HEAD.LASTDELV,
		T2_HEAD.SELLB, T2_HEAD.SELL1, Sum(T2_BRA.Q11) AS QTY1, Sum(T2_BRA.Q12) AS QTY2, Sum(T2_BRA.Q13) AS QTY3, 
			Sum(T2_BRA.Q14) AS QTY4, Sum(T2_BRA.Q15) AS QTY5, Sum(T2_BRA.Q16) AS QTY6, Sum(T2_BRA.Q17) AS QTY7, Sum(T2_BRA.Q18) AS QTY8, 
				Sum(T2_BRA.Q19) AS QTY9, Sum(T2_BRA.Q20) AS QTY10, Sum(T2_BRA.Q21) AS QTY11, Sum(T2_BRA.Q22) AS QTY12, Sum(T2_BRA.Q23) AS QTY13, 
					T2_HEAD.REF,Stocktype.MasterStocktype,SubDept.MasterSubDept, T2_HEAD.USER1,
                        Sum(T2_BRA.LY11) AS LY1, Sum(T2_BRA.LY12) AS LY2, Sum(T2_BRA.LY13) AS LY3, Sum(T2_BRA.LY14) AS LY4, Sum(T2_BRA.LY15) AS LY5, 
                            Sum(T2_BRA.LY16) AS LY6, Sum(T2_BRA.LY17) AS LY7, Sum(T2_BRA.LY18) AS LY8, Sum(T2_BRA.LY19) AS LY9, Sum(T2_BRA.LY20) AS LY10, 
                                Sum(T2_BRA.LY21) AS LY11, Sum(T2_BRA.LY22) AS LY12, Sum(T2_BRA.LY23) AS LY13, T2_HEAD.VAT, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM (((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN 

								(SELECT Mid(T2_LOOK.[KEY],4,6) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN

								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype
FROM T2_LOOK
WHERE (((Left([T2_LOOK]![KEY],3))='CAT'))
											ORDER BY Trim(Mid([T2_LOOK].[KEY],4,6))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) 	LEFT JOIN

									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK.[KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
									GROUP BY ([T2_BRA].[REF] + [F7]), Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.LASTDELV,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									  T2_HEAD.VAT, T2_HEAD.BASESELL, T2_HEAD.USER1,T2_HEAD.REM, T2_HEAD.REM2, T2_HEAD.MINSIZE, T2_HEAD.MAXSIZE,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1,T2_HEAD.REF,stocktype.MasterStocktype,SubDept.MasterSubDept,  T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC;";
		public static string AllSkusQuery =>
            @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT, 
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF, T2_HEAD.VAT, T2_HEAD.BASESELL, T2_HEAD.SELL,  T2_HEAD.USER1, T2_HEAD.DESC,
		T2_HEAD.SELLB, T2_HEAD.SELL1, Sum(T2_BRA.Q11) AS QTY1, Sum(T2_BRA.Q12) AS QTY2, Sum(T2_BRA.Q13) AS QTY3, 
			Sum(T2_BRA.Q14) AS QTY4, Sum(T2_BRA.Q15) AS QTY5, Sum(T2_BRA.Q16) AS QTY6, Sum(T2_BRA.Q17) AS QTY7, Sum(T2_BRA.Q18) AS QTY8, 
				Sum(T2_BRA.Q19) AS QTY9, Sum(T2_BRA.Q20) AS QTY10, Sum(T2_BRA.Q21) AS QTY11, Sum(T2_BRA.Q22) AS QTY12, Sum(T2_BRA.Q23) AS QTY13, 
					T2_HEAD.REF,Stocktype.MasterStocktype,SubDept.MasterSubDept,
                        Sum(T2_BRA.LY11) AS LY1, Sum(T2_BRA.LY12) AS LY2, Sum(T2_BRA.LY13) AS LY3, Sum(T2_BRA.LY14) AS LY4, Sum(T2_BRA.LY15) AS LY5, 
                            Sum(T2_BRA.LY16) AS LY6, Sum(T2_BRA.LY17) AS LY7, Sum(T2_BRA.LY18) AS LY8, Sum(T2_BRA.LY19) AS LY9, Sum(T2_BRA.LY20) AS LY10,
                                Sum(T2_BRA.LY21) AS LY11, Sum(T2_BRA.LY22) AS LY12, Sum(T2_BRA.LY23) AS LY13, T2_HEAD.VAT, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM (((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN 

								(SELECT Mid(T2_LOOK.[KEY],4,6) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN

								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype
FROM T2_LOOK
WHERE (((Left([T2_LOOK]![KEY],3))='CAT'))
											ORDER BY Trim(Mid([T2_LOOK].[KEY],4,6))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) 	LEFT JOIN

									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK.[KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
                                   WHERE [T2_HEAD].SELL <> [T2_HEAD].SELLB AND [T2_HEAD].SELLB <> 0 AND [T2_HEAD].SELL <> 0
									GROUP BY ([T2_BRA].[REF] + [F7]), Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.USER1, T2_HEAD.DESC,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									  T2_HEAD.VAT, T2_HEAD.BASESELL,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1,T2_HEAD.REF,stocktype.MasterStocktype,SubDept.MasterSubDept,  T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13, T2_HEAD.USER1, T2_HEAD.DESC
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC;";

        public static string SingleSkuSmaller =>
            @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, T2_SIZES.S01, T2_HEAD.[DESC], T2_HEAD.BASESELL, T2_HEAD.SELL,T2_HEAD.SELL1, T2_HEAD.SELLB, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM ((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN 
									 T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE))
                                    WHERE [T2_BRA].[REF] = ?
									GROUP BY ([T2_BRA].[REF] + [F7]), Colour.MasterColour, Colour.F7,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1, T2_HEAD.BASESELL,
									 T2_HEAD.REF, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC;";

        public static string AllSKUData =>
            @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, T2_SIZES.S01, [T2_BRA].[REF], T2_HEAD.[DESC], T2_HEAD.BASESELL, T2_HEAD.SELL,T2_HEAD.SELL1, T2_HEAD.SELLB, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM ((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN 
									 T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE))
									GROUP BY ([T2_BRA].[REF] + [F7]), Colour.MasterColour, Colour.F7, [T2_BRA].[REF],
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1, T2_HEAD.BASESELL,
									 T2_HEAD.REF, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC;";

        public static string SingleSku =>
            @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT, 
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF, T2_HEAD.VAT, T2_HEAD.BASESELL, T2_HEAD.SELL, 
		T2_HEAD.SELLB, T2_HEAD.SELL1, Sum(T2_BRA.Q11) AS QTY1, Sum(T2_BRA.Q12) AS QTY2, Sum(T2_BRA.Q13) AS QTY3, 
			Sum(T2_BRA.Q14) AS QTY4, Sum(T2_BRA.Q15) AS QTY5, Sum(T2_BRA.Q16) AS QTY6, Sum(T2_BRA.Q17) AS QTY7, Sum(T2_BRA.Q18) AS QTY8, 
				Sum(T2_BRA.Q19) AS QTY9, Sum(T2_BRA.Q20) AS QTY10, Sum(T2_BRA.Q21) AS QTY11, Sum(T2_BRA.Q22) AS QTY12, Sum(T2_BRA.Q23) AS QTY13, 
					T2_HEAD.REF,Stocktype.MasterStocktype,SubDept.MasterSubDept,
                        Sum(T2_BRA.LY11) AS LY1, Sum(T2_BRA.LY12) AS LY2, Sum(T2_BRA.LY13) AS LY3, Sum(T2_BRA.LY14) AS LY4, Sum(T2_BRA.LY15) AS LY5, 
                            Sum(T2_BRA.LY16) AS LY6, Sum(T2_BRA.LY17) AS LY7, Sum(T2_BRA.LY18) AS LY8, Sum(T2_BRA.LY19) AS LY9, Sum(T2_BRA.LY20) AS LY10, 
                                Sum(T2_BRA.LY21) AS LY11, Sum(T2_BRA.LY22) AS LY12, Sum(T2_BRA.LY23) AS LY13, T2_HEAD.VAT, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM (((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN 

								(SELECT Mid(T2_LOOK.[KEY],4,6) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN

								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype
FROM T2_LOOK
WHERE (((Left([T2_LOOK]![KEY],3))='CAT'))
											ORDER BY Trim(Mid([T2_LOOK].[KEY],4,6))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) 	LEFT JOIN

									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK.[KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
                                    WHERE [T2_BRA].[REF] = ? 
									GROUP BY ([T2_BRA].[REF] + [F7]), Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									  T2_HEAD.VAT, T2_HEAD.BASESELL,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1,T2_HEAD.REF,stocktype.MasterStocktype,SubDept.MasterSubDept,  T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC;";


        public static string CoreStockQuery =>
            @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, T2_BRA.REF, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT,
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.BASESELL, T2_HEAD.SELL,  T2_HEAD.USER1, T2_HEAD.SUPPREF,
		T2_HEAD.SELLB, T2_HEAD.SELL1, Stocktype.MasterStocktype,SubDept.MasterSubDept
									FROM ((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) 
									INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN 

								(SELECT Trim(Mid(T2_LOOK.[KEY],4,6)) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN 
								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype FROM T2_LOOK WHERE (((Left([T2_LOOK].[KEY],3))='CAT'))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) LEFT JOIN 
									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK![KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
                                   WHERE T2_BRA.BRANCH in ('A','B','G')";

        public static string CoreStockQueryOrder => @"GROUP BY ([T2_BRA].[REF] + [F7]), [T2_BRA].[REF], Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.USER1,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									 T2_HEAD.BASESELL,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1, stocktype.MasterStocktype,SubDept.MasterSubDept
									ORDER BY MasterSupplier ASC;";



        public static string GetItemList =>
            @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT,  T2_HEAD.USER1,
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF, T2_HEAD.VAT, T2_HEAD.BASESELL, T2_HEAD.SELL, 
		T2_HEAD.SELLB, T2_HEAD.SELL1, T2_HEAD.REF,Stocktype.MasterStocktype,SubDept.MasterSubDept, T2_HEAD.VAT, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM ((((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol)) INNER JOIN 

								(SELECT Mid(T2_LOOK.[KEY],4,6) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN

								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype
FROM T2_LOOK
WHERE (((Left([T2_LOOK]![KEY],3))='CAT'))
											ORDER BY Trim(Mid([T2_LOOK].[KEY],4,6))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) 	LEFT JOIN

									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK.[KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
                                    WHERE T2_BRA.BRANCH in ('A','G')
									GROUP BY ([T2_BRA].[REF] + [F7]), Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7,
									 T2_HEAD.SHORT, T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									  T2_HEAD.VAT, T2_HEAD.BASESELL,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1,T2_HEAD.REF,stocktype.MasterStocktype,SubDept.MasterSubDept,  T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13, T2_HEAD.USER1, T2_HEAD.DESC
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC";

        public static string GetItemListAllBranches =>
            @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT,  T2_HEAD.USER1, T2_HEAD.DESC,
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF, T2_HEAD.VAT, T2_HEAD.BASESELL, T2_HEAD.SELL, 
		T2_HEAD.SELLB, T2_HEAD.SELL1, T2_HEAD.REF,Stocktype.MasterStocktype,SubDept.MasterSubDept, T2_HEAD.VAT, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM ((((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN [DESC] ON [T2_BRA].[REF] = [DESC].SKU) INNER JOIN 

								(SELECT Mid(T2_LOOK.[KEY],4,6) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN

								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype
FROM T2_LOOK
WHERE (((Left([T2_LOOK]![KEY],3))='CAT'))
											ORDER BY Trim(Mid([T2_LOOK].[KEY],4,6))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) 	LEFT JOIN

									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK.[KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
									GROUP BY ([T2_BRA].[REF] + [F7]), Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									  T2_HEAD.VAT, T2_HEAD.BASESELL,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1,T2_HEAD.REF,stocktype.MasterStocktype,SubDept.MasterSubDept,  T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13, T2_HEAD.USER1, T2_HEAD.DESC
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC";


        public static string OnlineStock => @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT,  T2_HEAD.USER1, T2_HEAD.DESC,
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF, T2_HEAD.VAT, T2_HEAD.BASESELL, T2_HEAD.SELL,  Sum(T2_BRA.Q11) AS QTY1, Sum(T2_BRA.Q12) AS QTY2, Sum(T2_BRA.Q13) AS QTY3, 
			Sum(T2_BRA.Q14) AS QTY4, Sum(T2_BRA.Q15) AS QTY5, Sum(T2_BRA.Q16) AS QTY6, Sum(T2_BRA.Q17) AS QTY7, Sum(T2_BRA.Q18) AS QTY8, 
				Sum(T2_BRA.Q19) AS QTY9, Sum(T2_BRA.Q20) AS QTY10, Sum(T2_BRA.Q21) AS QTY11, Sum(T2_BRA.Q22) AS QTY12, Sum(T2_BRA.Q23) AS QTY13, 
 Sum(T2_BRA.LY11) AS LY1, Sum(T2_BRA.LY12) AS LY2, Sum(T2_BRA.LY13) AS LY3, Sum(T2_BRA.LY14) AS LY4, Sum(T2_BRA.LY15) AS LY5, 
                            Sum(T2_BRA.LY16) AS LY6, Sum(T2_BRA.LY17) AS LY7, Sum(T2_BRA.LY18) AS LY8, Sum(T2_BRA.LY19) AS LY9, Sum(T2_BRA.LY20) AS LY10, 
                                Sum(T2_BRA.LY21) AS LY11, Sum(T2_BRA.LY22) AS LY12, Sum(T2_BRA.LY23) AS LY13,
		T2_HEAD.SELLB, T2_HEAD.SELL1, T2_HEAD.REF,Stocktype.MasterStocktype,SubDept.MasterSubDept, T2_HEAD.VAT, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM ((((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN [DESC] ON [T2_BRA].[REF] = [DESC].SKU) INNER JOIN 

								(SELECT Mid(T2_LOOK.[KEY],4,6) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN

								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype
FROM T2_LOOK
WHERE (((Left([T2_LOOK]![KEY],3))='CAT'))
											ORDER BY Trim(Mid([T2_LOOK].[KEY],4,6))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) 	LEFT JOIN

									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK.[KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
									GROUP BY ([T2_BRA].[REF] + [F7]), Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									  T2_HEAD.VAT, T2_HEAD.BASESELL,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1,T2_HEAD.REF,stocktype.MasterStocktype,SubDept.MasterSubDept,  T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13, T2_HEAD.USER1, T2_HEAD.DESC
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC;";

		public static string OnlineSingleStock => @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT,  T2_HEAD.USER1, T2_HEAD.DESC,
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF, T2_HEAD.VAT, T2_HEAD.BASESELL, T2_HEAD.SELL,  Sum(T2_BRA.Q11) AS QTY1, Sum(T2_BRA.Q12) AS QTY2, Sum(T2_BRA.Q13) AS QTY3, 
			Sum(T2_BRA.Q14) AS QTY4, Sum(T2_BRA.Q15) AS QTY5, Sum(T2_BRA.Q16) AS QTY6, Sum(T2_BRA.Q17) AS QTY7, Sum(T2_BRA.Q18) AS QTY8, 
				Sum(T2_BRA.Q19) AS QTY9, Sum(T2_BRA.Q20) AS QTY10, Sum(T2_BRA.Q21) AS QTY11, Sum(T2_BRA.Q22) AS QTY12, Sum(T2_BRA.Q23) AS QTY13, 
 Sum(T2_BRA.LY11) AS LY1, Sum(T2_BRA.LY12) AS LY2, Sum(T2_BRA.LY13) AS LY3, Sum(T2_BRA.LY14) AS LY4, Sum(T2_BRA.LY15) AS LY5, 
                            Sum(T2_BRA.LY16) AS LY6, Sum(T2_BRA.LY17) AS LY7, Sum(T2_BRA.LY18) AS LY8, Sum(T2_BRA.LY19) AS LY9, Sum(T2_BRA.LY20) AS LY10, 
                                Sum(T2_BRA.LY21) AS LY11, Sum(T2_BRA.LY22) AS LY12, Sum(T2_BRA.LY23) AS LY13,
		T2_HEAD.SELLB, T2_HEAD.SELL1, T2_HEAD.REF,Stocktype.MasterStocktype,SubDept.MasterSubDept, T2_HEAD.VAT, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM ((((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN [DESC] ON [T2_BRA].[REF] = [DESC].SKU) INNER JOIN 

								(SELECT Mid(T2_LOOK.[KEY],4,6) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN

								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype
FROM T2_LOOK
WHERE (((Left([T2_LOOK]![KEY],3))='CAT'))
											ORDER BY Trim(Mid([T2_LOOK].[KEY],4,6))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) 	LEFT JOIN

									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK.[KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
									WHERE [T2_BRA].[REF] = ? 
									GROUP BY ([T2_BRA].[REF] + [F7]), Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									  T2_HEAD.VAT, T2_HEAD.BASESELL,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1,T2_HEAD.REF,stocktype.MasterStocktype,SubDept.MasterSubDept,  T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13, T2_HEAD.USER1, T2_HEAD.DESC
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC;";

		public static string StockQuery =>
            @"SELECT ([T2_BRA].[REF] + [F7]) AS NEWSTYLE, Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.SHORT, 
	T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF, T2_HEAD.VAT, T2_HEAD.BASESELL, T2_HEAD.SELL, 
		T2_HEAD.SELLB, T2_HEAD.SELL1, T2_HEAD.REF,Stocktype.MasterStocktype,SubDept.MasterSubDept, T2_HEAD.USER1, T2_HEAD.DESC,  Sum(T2_BRA.Q11) AS QTY1, Sum(T2_BRA.Q12) AS QTY2, Sum(T2_BRA.Q13) AS QTY3, 
			Sum(T2_BRA.Q14) AS QTY4, Sum(T2_BRA.Q15) AS QTY5, Sum(T2_BRA.Q16) AS QTY6, Sum(T2_BRA.Q17) AS QTY7, Sum(T2_BRA.Q18) AS QTY8, 
				Sum(T2_BRA.Q19) AS QTY9, Sum(T2_BRA.Q20) AS QTY10, Sum(T2_BRA.Q21) AS QTY11, Sum(T2_BRA.Q22) AS QTY12, Sum(T2_BRA.Q23) AS QTY13, 
 Sum(T2_BRA.LY11) AS LY1, Sum(T2_BRA.LY12) AS LY2, Sum(T2_BRA.LY13) AS LY3, Sum(T2_BRA.LY14) AS LY4, Sum(T2_BRA.LY15) AS LY5, 
                            Sum(T2_BRA.LY16) AS LY6, Sum(T2_BRA.LY17) AS LY7, Sum(T2_BRA.LY18) AS LY8, Sum(T2_BRA.LY19) AS LY9, Sum(T2_BRA.LY20) AS LY10, 
                                Sum(T2_BRA.LY21) AS LY11, Sum(T2_BRA.LY22) AS LY12, Sum(T2_BRA.LY23) AS LY13,
                       T2_HEAD.VAT, T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									FROM (((((((T2_BRA INNER JOIN T2_HEAD ON T2_BRA.REF = T2_HEAD.REF) INNER JOIN (SELECT Right(T2_LOOK.[KEY],3) AS NewCol, T2_LOOK.F1 AS MasterColour, Left(T2_LOOK.[KEY],3) AS Col, T2_LOOK.F7
								FROM T2_LOOK
								WHERE (Left(T2_LOOK.[KEY],3))='COL') as Colour ON T2_BRA.COLOUR = Colour.NewCol) INNER JOIN 

								(SELECT Mid(T2_LOOK.[KEY],4,6) AS SuppCode, T2_LOOK.F1 AS MasterSupplier
									FROM T2_LOOK
										WHERE (((Left(T2_LOOK.[KEY],3))='SUP'))
											) as  Suppliers ON T2_HEAD.SUPPLIER = Suppliers.SuppCode) INNER JOIN T2_SIZES ON T2_HEAD.SIZERANGE = T2_SIZES.SIZERANGE) INNER JOIN

											(SELECT Right([T2_LOOK].[KEY],3) AS DeptCode, T2_LOOK.F1 AS MasterDept
												FROM T2_LOOK
													WHERE (Left([T2_LOOK].[KEY],3))='TYP') As Dept ON T2_HEAD.STYPE = Dept.DeptCode) INNER JOIN

								(SELECT Trim(Mid([T2_LOOK]![KEY],4,6)) AS StkType, T2_LOOK.F1 AS MasterStocktype
FROM T2_LOOK
WHERE (((Left([T2_LOOK]![KEY],3))='CAT')) ORDER BY Trim(Mid([T2_LOOK].[KEY],4,6))) as Stocktype
									ON T2_HEAD.[GROUP] = Stocktype.StkType) LEFT JOIN 
									(SELECT Right(T2_LOOK.[KEY],3) AS SubDeptCode, T2_LOOK.F1 AS MasterSubDept
										FROM T2_LOOK
											WHERE (Left(T2_LOOK.[KEY],3))='US2') AS SubDept ON T2_HEAD.USER2 = SubDept.SubDeptCode)
                                   WHERE USER1 <> 'S16' AND USER1 <> 'W16' AND USER1 <> 'S15' AND USER1 <> 'W15' AND USER1 <> 'S14' AND USER1 <> 'W14' AND USER1 <> 'S13'  AND USER1 <> 'W13'
								   AND USER1 <> 'B16' AND USER1 <> 'B15' AND USER1 <> 'B14' AND USER1 <> 'B13' AND USER1 <> 'B12' AND USER1 <> 'B11' AND USER1 <> 'S12' AND USER1 <> 'W12' 
								   AND USER1 <> 'S11' AND USER1 <> 'W11'
									GROUP BY ([T2_BRA].[REF] + [F7]), Suppliers.MasterSupplier, Dept.MasterDept, Colour.MasterColour, Colour.F7, T2_HEAD.USER1, T2_HEAD.DESC,
									 T2_HEAD.SHORT, T2_HEAD.[DESC], T2_HEAD.[GROUP], T2_HEAD.STYPE, T2_HEAD.SIZERANGE, T2_HEAD.SUPPLIER, T2_HEAD.SUPPREF,
									  T2_HEAD.VAT, T2_HEAD.BASESELL,
									 T2_HEAD.SELL, T2_HEAD.SELLB, T2_HEAD.SELL1,T2_HEAD.REF,stocktype.MasterStocktype,SubDept.MasterSubDept,  T2_SIZES.S01, T2_SIZES.S02, T2_SIZES.S03, T2_SIZES.S04, T2_SIZES.S05, 
									 T2_SIZES.S06, T2_SIZES.S07, T2_SIZES.S08, T2_SIZES.S09, T2_SIZES.S10, T2_SIZES.S11, T2_SIZES.S12, T2_SIZES.S13
									ORDER BY ([T2_BRA].[REF] + [F7]) DESC;";

        public static string InsertSKU => @"INSERT INTO [DESC] (SKU) VALUES (?)";

        public static string DeleteSKUs => @"DELETE FROM [DESC]";

        public static string InsertREM => @"INSERT INTO [REM] (NAME,REM,ID,PROPERTY) VALUES (@name, @rem, @id, @property)";

        public static string FetchREM => @"SELECT * FROM [REM] where REM = ?";

        public static string DeleteREM => @"DELETE FROM [REM] PACK";

        public static string FetchSeasonalData => @"SELECT * FROM [CONFIG]";

        public static string InsertSeasonalData => @"INSERT INTO [CONFIG] ([SEASON],[ID],[TOPPAGE],[BOTTOMPAGE]) VALUES (@season, @id, @toppage, @bottompage)";

        public static string DeleteConfigurables => @"DELETE FROM [CONFIG]";

		public static string PackREM => @"Pack [REM]";

		public static string FetchEUROPrice => @"SELECT [PRICE] FROM [EURO]";

		public static string FetchSaleInfo => @"SELECT * FROM [SALES] WHERE [SKU] = ?";

		public static string FetchAllSales => @"SELECT * FROM [SALES]";

		public static string InsertSalesSKU => @"INSERT INTO [SALES] ([STOREID], [SKU], [PRICE], [START], [END]) VALUES (@storeid, @sku, @price, @start, @end)";

		public static string DeleteSales => @"DELETE FROM [SALES] WHERE [SKU] = ? AND [STOREID] = storeid";

	}
}
