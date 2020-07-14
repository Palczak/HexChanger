using Hexes;
using Instructions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Managers
{
    public class FileManager
    {
        public HexIO HexIO { get; }
        public string InstructionDir { get; set; }
        private readonly string _identifyName;
        public string IdentifyName { get { return _identifyName; } }
        public readonly string FindName;
        private readonly string _replaceName;
        private readonly string _fillName;
        private readonly string _beforeName;
        private readonly string _insideName;
        private readonly string _afterName;

        public FileManager()
        {
            HexIO = new HexIO();
            InstructionDir = "";
            _identifyName = "ident";
            FindName = "find";
            _replaceName = "replace";
            _fillName = "fill";
            _beforeName = "before";
            _insideName = "inside";
            _afterName = "after";
        }

        public void SaveResult(string path, Hex result)
        {
            HexIO.WriteHex(path, result);
        }

        public Hex ReadCorruptedHex(string path)
        {
            return HexIO.ReadHex(path);
        }

        public InstructionSet ReadInstructions()
        {
            if (!IsInstructionSet())
            {
                throw new Exception("Instrukcje nie zostały ustawione.");
            }
            InstructionSet resultSet = new InstructionSet
            {
                Identify = ReadIdentify(),
                FindList = ReadFind(),
                ReplaceList = ReadReplace(),
                FillList = ReadFill()
            };
            return resultSet;
        }

        private Identify ReadIdentify()
        {
            string[] rawPaths = Directory.GetFiles(InstructionDir);

            if (rawPaths.Length == 0)
            {
                throw new IOException("Nie znaleziono plików w katalogu z instrukcjami.");
            }

            string identifyPath = null;
            Regex rx = new Regex(_identifyName + ".*");
            foreach (string path in rawPaths)
            {
                if (rx.IsMatch(Path.GetFileName(path)))
                {
                    if (identifyPath != null)
                    {
                        throw new Exception("Jest więcej niż 1 plik identyfikacyjny " + _identifyName + ".");
                    }
                    identifyPath = path;
                }
            }

            if (identifyPath == null)
            {
                throw new Exception("Nie znaleziono pliku identyfikacyjnego " + _identifyName + ".");
            }

            Hex identifyHex = HexIO.ReadHex(identifyPath);
            return new Identify(identifyHex);
        }

        private List<Find> ReadFind()
        {
            Regex rx = new Regex(FindName + "\\d{3}.*");
            var resultFindList = new List<Find>();

            string findDir = Path.Combine(InstructionDir, FindName);
            if(!Directory.Exists(findDir))
            {
                throw new Exception("Nie znaleziono katalogu z plikami szukającymi.");
            }
            string[] rawPaths = Directory.GetFiles(findDir);
            if (rawPaths.Length == 0)
            {
                throw new Exception("Nie znaleziono ani jednego pliku szukającego " + FindName + ".");
            }
            foreach (string path in rawPaths)
            {
                string name = Path.GetFileName(path);
                if (rx.IsMatch(name))
                {
                    int index = int.Parse(name.Substring(FindName.Length, 3));
                    resultFindList.Add(new Find(HexIO.ReadHex(path), index));
                }
            }
            return resultFindList;
        }

        private List<Replace> ReadReplace()
        {
            var rx = new Regex(_replaceName + "\\d{3}.*");
            var resultReplaceList = new List<Replace>();

            string replaceDir = InstructionDir;
            if (!Directory.Exists(replaceDir))
            {
                throw new Exception("Nie znaleziono katalogu z plikami zamieniającymi.");
            }
            string[] rawPaths = Directory.GetDirectories(replaceDir);
            if (rawPaths.Length == 0)
            {
                throw new Exception("Nie znaleziono ani jednego pliku zamieniającego " + _replaceName + ".");
            }
            foreach (string path in rawPaths)
            {
                string name = Path.GetFileName(path);
                if (rx.IsMatch(name))
                {
                    int index = int.Parse(name.Substring(_replaceName.Length, 3));
                    Hex beforeHex = null;
                    Hex insideHex = null;
                    Hex afterHex = null;

                    string[] rawInstructionPaths = Directory.GetFiles(path);
                    var rxBefore = new Regex(_beforeName + ".*");
                    var rxInside = new Regex(_insideName + ".*");
                    var rxAfter = new Regex(_afterName + ".*");
                    foreach (string insidePath in rawInstructionPaths)
                    {
                        if (rxBefore.IsMatch(Path.GetFileName(insidePath)))
                        {
                            if (beforeHex != null)
                            {
                                throw new Exception("Jest wiecej niż 1 plik do zamiany przed " + _beforeName + " w katalogu " + insidePath + ".");
                            }
                            beforeHex = HexIO.ReadHex(insidePath);
                        }
                        else if (rxInside.IsMatch(Path.GetFileName(insidePath)))
                        {
                            if (insideHex != null)
                            {
                                throw new Exception("Jest wiecej niż 1 plik do zamiany w środku " + _insideName + " w katalogu " + insidePath + ".");
                            }
                            insideHex = HexIO.ReadHex(insidePath);

                        }
                        else if (rxAfter.IsMatch(Path.GetFileName(insidePath)))
                        {
                            if (afterHex != null)
                            {
                                throw new Exception("Jest wiecej niż 1 plik do zamiany po " + _afterName + " w katalogu " + insidePath + ".");
                            }
                            afterHex = HexIO.ReadHex(insidePath);
                        }
                    }
                    Replace newReplace = new Replace(beforeHex, insideHex, afterHex, index);
                    resultReplaceList.Add(newReplace);
                }
            }
            return resultReplaceList;
        }

        private List<Fill> ReadFill()
        {
            var rx = new Regex(_fillName + "\\d{3}.*");
            var resultFillList = new List<Fill>();

            string fillDir = InstructionDir;
            if (!Directory.Exists(fillDir))
            {
                throw new Exception("Nie znaleziono katalogu z plikami uzupełniającymi.");
            }
            string[] rawPaths = Directory.GetDirectories(fillDir);
            if (rawPaths.Length == 0)
            {
                throw new Exception("Nie znaleziono ani jednego pliku uzupełniającego " + _fillName + ".");
            }
            foreach (string path in rawPaths)
            {
                string name = Path.GetFileName(path);
                if (rx.IsMatch(name))
                {
                    int index = int.Parse(name.Substring(_fillName.Length, 3));
                    Hex beforeHex = null;
                    Hex insideHex = null;
                    Hex afterHex = null;

                    string[] rawInstructionPaths = Directory.GetFiles(path);
                    var rxBefore = new Regex(_beforeName + ".*");
                    var rxInside = new Regex(_insideName + ".*");
                    var rxAfter = new Regex(_afterName + ".*");
                    foreach (string insidePath in rawInstructionPaths)
                    {
                        if (rxBefore.IsMatch(Path.GetFileName(insidePath)))
                        {
                            if (beforeHex != null)
                            {
                                throw new Exception("Jest wiecej niż 1 plik do uzupełnienia przed " + _beforeName + " w katalogu " + insidePath + ".");
                            }
                            beforeHex = HexIO.ReadHex(insidePath);
                        }
                        else if (rxInside.IsMatch(Path.GetFileName(insidePath)))
                        {
                            if (insideHex != null)
                            {
                                throw new Exception("Jest wiecej niż 1 plik do uzupełnienia w środku " + _insideName + " w katalogu " + insidePath + ".");
                            }
                            insideHex = HexIO.ReadHex(insidePath);

                        }
                        else if (rxAfter.IsMatch(Path.GetFileName(insidePath)))
                        {
                            if (afterHex != null)
                            {
                                throw new Exception("Jest wiecej niż 1 plik do uzupełnienia po " + _afterName + " w katalogu " + insidePath + ".");
                            }
                            afterHex = HexIO.ReadHex(insidePath);
                        }
                    }
                    var newFill = new Fill(beforeHex, insideHex, afterHex, index);
                    resultFillList.Add(newFill);
                }
            }
            return resultFillList;
        }

        bool IsInstructionSet()
        {
            return InstructionDir != null;
        }

        bool IsSet()
        {
            return IsInstructionSet();
        }
    }
}
