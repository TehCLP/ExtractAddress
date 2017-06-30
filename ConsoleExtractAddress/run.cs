using cExtractAddr;
using cSourceData;
using System;
using System.Collections.Generic;
using System.Data; 
using System.Linq;
using System.Text;

namespace ConsoleExtractAddress
{
    internal class run
    {
        public static void Extract()
        {
            using (var spinner = new ConsoleSpinner())
            {
                spinner.Start();

                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                System.Diagnostics.Stopwatch stopwatch2 = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                writeConsole("#####################################");
                writeConsole("######                         ######");
                writeConsole("######  Start Extract Address  ######");
                writeConsole("######                         ######");
                writeConsole("#####################################"); 
                writeConsole("-------------------------------------");
                writeConsole();

                using (DataTable dtProvince = db.getDataProvince())
                {
                    using (DataTable dtYears = db.getYearCreateAddress())
                    {
                        DataTable dt;
                        DataTable dtAddress;
                        string _year;
                        AddressManager _ex = new AddressManager(dtProvince);
                        foreach (DataRow _dr in dtYears.Rows)
                        {
                            stopwatch2.Start();

                            try
                            {
                                _year = Convert.ToString(_dr["years"]);
                                dtAddress = db.getDataAddress(_year);

                                writeConsole(string.Format("=> GetAddress from year : {0}", _dr["years"]), consoleType.info);
                                writeConsole(string.Format("=> Total Rows : {0}", dtAddress.Rows.Count.ToString("n0")), consoleType.info);
                                writeConsole(string.Format("=>> GetAddress Success : {0}", stopwatch2.Elapsed), consoleType.warn);
                                writeConsole();

                                if (dtAddress != null && dtAddress.Rows.Count > 0)
                                {
                                    writeConsole(string.Format("=>> Start Extract Address   : {0}", stopwatch2.Elapsed));
                                    dt = _ex.eXtract(dtAddress, _year);
                                    writeConsole(string.Format("=>> Extract Address Success : {0}", stopwatch2.Elapsed), consoleType.warn);
                                    writeConsole();

                                    writeConsole(string.Format("=>> Start Clear Exists Data   : {0}", stopwatch2.Elapsed));
                                    db.clearExistsDataIn_TbExtract(dt);
                                    writeConsole(string.Format("=>> Clear Exists Data Success : {0}", stopwatch2.Elapsed), consoleType.warn);
                                    writeConsole();

                                    writeConsole(string.Format("=>> Start Insert into DB   : {0}", stopwatch2.Elapsed));
                                    db.insertBulkToImportDB("tempAddress", dt);
                                    writeConsole(string.Format("=>> Insert into DB Success : {0}", stopwatch2.Elapsed), consoleType.warn);
                                    writeConsole();

                                    dt.Dispose();                                    
                                }
                                dtAddress.Dispose();

                                writeConsole("=>>> Success \\(^.^)/", consoleType.success);

                            }
                            catch (Exception ex)
                            {
                                writeConsole("=>>> Error /(-_-\")\\ ", consoleType.error);
                                writeConsole(string.Format("=>>> : {0}", ex.Message), consoleType.error);
                            }

                            writeConsole("-------------------------------------");
                            writeConsole();

                            stopwatch2.Stop();
                            stopwatch2.Reset();

                            //break;
                        }
                    }
                }

                stopwatch.Stop(); 
                writeConsole("-------------------------------------");
                writeConsole(string.Format("### Time elapsed: {0} ###", stopwatch.Elapsed));

            }

            Console.ReadLine();
        }

        public static void Extract_debug()
        {
            DataTable dtProvince = db.getDataProvince();
            AddressManager _ex = new AddressManager(dtProvince);

            string _testAddr = string.Empty;
            _testAddr = "111/11-A LIBERTY PARK #11A SOI SUKHUMVIT23, KLONGTOEY NUA WATTANA, BANGKOK ranong";
            //_testAddr = "11/22 The Next Condominium  ซ.สุขุมวิท 52 ถ.สุขุมวิท แขวงบางจาก  เขตพระโขนง กทม.";
            //_testAddr = "บจก. เครื่องพ่นไฟ (สำนักงานใหญ่มาก) เลขที่ 23/456 หมู่ 7 ต.อ้อมใหญ่ อ.สามพราน นครปฐม";
            //_testAddr = "888/88 ม.8 ถ.อำเภอ ต.มะขามเตี้ย อำเภอ เมือง จ.สุราษฎร์ธานี                            ";
            //_testAddr = "10 (88/88) ซ.พระรามที่ 2 ซอย 69 แยก 4-11 แขวงแสมดำ เขตบางขุนเทียน กทม.";
            //_testAddr = "818/18 ถ.ประชาชื่น  เขตบางซื่อ กรุงเทพมหานคร";
            //_testAddr = "บริษัท  ฟู้ดแอนท์แฟน 2 จำกัด 10/11. หมู่ที่ 2 อาคารเซ็นทรัล แจ้งวัฒนะ ออฟฟิศ ชั้น 17C ถ.แจ้งวัฒนะ ต.บางตลาด อ.ปากเกร็ด นนทบุรี";
            //_testAddr = "เทสเตอร์-พระราม 5 บางใหญ่ 11/1234 ม.4 ถ.กาญจนาภิเษก ต.บางแม่นาง อ.บางใหญ่ จ.นนทบุรี";
            //_testAddr = "123/4 หมู่ที่ 5 หมู่บ้าน ลมโชย อาคารลมร้อน ชั้น ลมหนาว ซอย ลมทะเล ถนน หาดทราย ยานนาวา เขตสาทร กรุงเทพมหานคร";
            //_testAddr = "55 หมู่5  ต.บ้านระกาศ อ.บางบ่อ จ.สมุทรปราการ";  
            //_testAddr = "พร้อมพันธุ์ 2 1 ซ.3 ถ.ลาดพร้าว 3 แขวงจอมพล เขตจตุจักร กรุงเทพมหานคร";
            //_testAddr = "บล.เอบีซีดีอี (นะจ๊ะ) จำกัด เลขที่ 87/6 อาคารซีอาร์ซี ทาวเว่อร์ ชั้นที่ 28, 49 ออลซีซั่น เพลส ถนนวิทยุ แขวงลุมพินี เขตปทุมวัน กรุงเทพมหานคร";
            //_testAddr = "321 ม.7 ถ.เลย-หล่มสัก ต.หนองบัว อ.ภูเรือ จ.เลย";
            //_testAddr = "อาคารสทิสโก้ทาวเวอร์ ชั้น 1 123-128 ถ.สาทรเหนือ แขวงสีลม เขตบางรัก กทม.";
            //_testAddr = "999 อาคารอับดุลราฮิม ชั้น 5 และชั้น 22-25 ถ.พระราม 4 แขวงสีลม เขตบางรัก กทม.";
            //_testAddr = "888 อาคารเพลินจิตทาวเวอร์ ชั้น 1 โซนนี้ ชั้น 2 โซนนั้น และชั้น 3 โซนไหน ถ.เพลินจิต แขวงลุมพินี เขตปทุมวัน กทม.";
            //_testAddr = "10/19-22,11/20-62 อาคารพีเอสทาวเวอร์ ชั้นที่ 12 และ 18 ถ.สุขุมวิท 21 (อโศก) แขวงคลองเตยเหนือ เขตวัฒนา กทม.";
            //_testAddr = "12-123 อาคารสินธรทาวเวอร์ 2 ชั้น 1, 2 และ อาคารสินธรทาวเวอร์ 3 ชั้น 3 ถ.วิทยุ แขวงลุมพินี เขตปทุมวัน กทม."; 
            //_testAddr = "บจก.วิศวกรรมการซ่อมรัก   69 (แผนกEngineer ชั้น 89 ซ้ายมือ) ถ.สุขุมวิท  ต.มาบตาพุด อ.เมือง จ.ระยอง";
            //_testAddr = "บมจ.หาปลามาเซอร์วิส เลขที่ 1221/1 ชั้น16 ส่วนหน้า อาคารชินวัตร 2 ถ.พหลโยธิน แขวงสามเสนใน เขตพญาไท กทม.";
            //_testAddr = "63 ธนาคารอาคารไม้ (สำนักงานใหญ่) ชั้น15 อาคาร 2 (สำนักงาน นี้นะ.) แขวง/เขตห้วยขวาง กทม."; 
            //_testAddr = "100 ลีโอทาวเวอร์นึง ชั้น 13A ส่วน LMA ถ.สาทรเหนือ แขวงสีลม เขตบางรัก กทม.";
            //_testAddr = "123 อาคารเบอร์เล่อร์ ชั้น 12 (ส่วนบริหารบุคคล) แขวงดินแดง เขตดินแดง กทม.";
            //_testAddr = "ABCDEF Tower (ชั้น 14 ส่วนหน้า) 1221/1 ถ.พหลโยธิน แขวงสามเสนใน เขตพญาไท กรุงเทพฯ";
            //_testAddr = "C/C ฝ่ายลูกค้าสถาบัน 888 อาคารเพลินจิตทาวเวอร์ ชั้น1-2 โซน เอ,ชั้น 12 ถ.เพลินจิต แขวงลุมพินี เขตปทุมวัน กทม.";
            //_testAddr = "C/C ฝ่ายลูกค้าสถาบัน บลจ.ช้างกูอยู่ไหน จำกัด 888 อาคารเพลินจิตทาววเวอร์ ชั้น 1 โซน A ชั้น 2 โซน A ชั้น 12 ถ.เพลินจิต แขวงลุมพินี เขตปทุมวัน กทม.";
            //_testAddr = "2468 อาคารอิสระจังแว้ ชั้น 3 โซนเอ-บี ถ.เพชรบุรีตัดใหม่ แขวงบางกะปิ เขตห้วยขวาง กทม.";
            //_testAddr = "ร้าน EE อาคารถูไถทาวเวอร์ 1 ชั้น 4 โซน 2B  ถ.ดำรงษ์ แขวงป้อมปราบ เขตป้อมปราบศัตรูพ่าย กทม.";
            //_testAddr = "บริษัท อยากกินหอย (ประเทศไทย) จำกัด 69 อาคารชนแล้วหนีทาวเวอร์ ชั้น 12 โซนเอ,บี ถ.เพชรบุรีตัดใหม่ แขวงมักกะสัน เขตราชเทวี กทม.";   
            //_testAddr = "ธ.กรุงมั้ย (ถนนเพชรบุรีตัดใหม่) 2170 อาคารกรุงมั้ยทาวเวอร์ ชั้น G ถ.เพชรบุรีตัดใหม่ แขวงบางกะปิ เขตห้วยขวาง กทม.";
            //_testAddr = "105 พระราม 9 ซ.60 ( ซ.7 เสรี 7 ) ถ.พระราม 9 แขวง / เขตสวนหลวง กทม.";
            //_testAddr = "111/1 หมู่11 ซ.บ้านบัวบาน-บัวโรย ถ.หลวงแพ่ง ต./อ. บางเสาธง สมุทรปราการ";   
            //_testAddr = "ภาควิชากายวิภาคศาสตร์ คณะแพทยศา สตร์ศิริราชพยาบาล มหาวิทยาลัยมหิดล แขวงศิริราช เขตบางกอกน้อย ก.ท.ม.";
            //_testAddr = "ธนาคารธรรมดา จำกัด (มหาชน) สายปฏิบัติธรรมและลักทรัพย์ อาคาร 2 ชั้น 3 ,1060 ถ.เพชรบุรี เขตราชเทวี กทม.";
            //_testAddr = "บ.อยากขับบิ๊กไบซ์(ไทยแลนด์) 444 อาคารอยู่ลิมตึกไทยชั้น9ถ.รัชดาภิเษก แขวงสามเสนนอก เขตห้วยขวาง กทม.";
            //_testAddr = "บ.ดาวน์แล้วไม่ผ่อน ประเทศไทย จำกัด อาคารกรุ๊ปบายไอดีสิชั้น 15, 75 ซ.รูเบีย สุขุมวิท 42 พระโขนง คลองเตย กทม.";
            _testAddr = "1234 อาคารบอดี้สลิม ขั้น 22 ถนนสีลม เขตบางรัก กรุงเทพมหานคร";

            _testAddr = _ex.manageStrAddress(_testAddr);

            //var _p = _ex.extractProvince_Eng(_testAddr);
            //var _n = _ex.extractNo_Eng(_testAddr);

            string[] _province = _ex.extractProvince(ref _testAddr);
            string _amphor = _ex.extractAmphor(ref _testAddr, _province[0], _province[1]);
            string _tambon = _ex.extractTambon(ref _testAddr, _amphor, _province[1]);
            string _floor = _ex.extractFloor(ref _testAddr);
            string _road = _ex.extractRoad(ref _testAddr);
            string _soi = _ex.extractSoi(ref _testAddr); 
            string _moo = _ex.extractMoo(ref _testAddr);
            string _no = _ex.extractNo(ref _testAddr);
        } 
        
        private enum consoleType
        {
            info,
            success,
            error,
            warn,
            normal
        }
        private static void writeConsole(string msg = "", consoleType cType = consoleType.normal)
        {
            switch (cType)
            {
                case consoleType.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case consoleType.warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case consoleType.info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case consoleType.success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default :
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine(msg);
            Console.ResetColor();
        }

    }
}
