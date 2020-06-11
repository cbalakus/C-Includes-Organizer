using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Edit_61850_Includes
{
    class Program
    {
        static void Main(string[] args)
        {
            string startPath = @"C:\Users\can.alakus\Desktop\cMake\iec61850";
            setDirectories(startPath);
            setIncludes(startPath);
            Console.WriteLine("Finished!");
            Console.ReadKey();
        }
        static void setIncludes(string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
                setIncludes(directory);
            foreach (var file in Directory.GetFiles(path))
                if (file.EndsWith(".h") || file.EndsWith(".c"))
                {
                    var lines = new List<string>();
                    foreach (var line in File.ReadAllLines(file))
                    {
                        string lineTrimmed = line.Trim();
                        if (lineTrimmed.StartsWith("#include"))
                        {
                            string newStr = line;
                            string headerName = lineTrimmed.Replace("#include", "").Trim();
                            if (headerName.StartsWith("<"))
                            {
                                var newLine = newStr.Replace("<", "\"").Replace(">", "\"");
                                lines.Add(newLine);
                            }
                            else
                                lines.Add(line);
                        }
                        else
                            lines.Add(line);
                    }
                    File.WriteAllLines(file, lines.ToArray());
                }
        }

        static void findTheWay(string[] ary1, string[] ary2, ref string retVal)
        {
            for (int i = 0; i < ary1.Length; i++)
                if (ary2[i] == ary1[i])
                    retVal = new Regex(Regex.Escape(ary1[i] + "\\")).Replace(retVal, "", 1);
                else
                    break;
        }

        static string replaceFirst(string str1, string str2)
        {
            string retVal = str2;
            var list1 = str1.Split('\\');
            var list2 = str2.Split('\\');
            if (list1.Length >= list2.Length)
                findTheWay(list2, list1, ref retVal);
            else
                findTheWay(list1, list2, ref retVal);
            return retVal;
        }
        static string getParentStr(int len)
        {
            string retVal = "";
            for (int i = 0; i < len - 1; i++)
                retVal += "../";
            return retVal;
        }
        static void setDirectories(string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
                setDirectories(directory);
            foreach (var file in Directory.GetFiles(path))
            {
                if (file.EndsWith(".h") || file.EndsWith(".c"))
                {
                    var lines = new List<string>();
                    foreach (var line in File.ReadAllLines(file))
                    {
                        string lineTrimmed = line.Trim();
                        if (lineTrimmed.StartsWith("#include"))
                        {
                            string newStr = line;
                            string headerName = lineTrimmed.Replace("#include", "").Trim();
                            if (headerName.StartsWith("\"") && !headerName.Contains("/") && !headerName.Contains("\\"))
                            {
                                string include = findIncludePath(headerName, file);
                                if (include != "")
                                {
                                    string includeStr = parentQtt == 0 ?
                                        replaceFirst(file, include) :
                                        getParentStr(parentQtt) + replaceFirst(file, include).Replace("\\", "/");
                                    parentQtt = 0;
                                    if (includeStr != "")
                                        newStr = line.Replace(headerName.Replace("\"", ""), includeStr);
                                }
                                else
                                    Console.WriteLine("Not found : " + file + " - " + headerName);
                            }
                            lines.Add(newStr);
                        }
                        else
                            lines.Add(line);
                    }
                    File.WriteAllLines(file, lines.ToArray());
                }
            }
        }

        static string findIncludePath(string header, string startPos) =>
             findLocation(header, startPos.Substring(0, startPos.LastIndexOf("\\")));

        static string findLocation(string fileName, string path)
        {
            fileName = fileName.Replace("\"", "");
            if (!path.EndsWith("cMake"))
            {
                var files = Directory.GetFiles(path);
                var file = files.Where(w => w.EndsWith(fileName)).FirstOrDefault();
                if (file != null)
                    return file;
                else
                {
                    foreach (var directory in Directory.GetDirectories(path))
                    {
                        string foundedLoc = findLocation(fileName, directory);
                        if (foundedLoc != "")
                            return foundedLoc;
                    }
                    parentQtt = 0;
                    var parentStr = searchInParent(fileName, path);
                    if (parentStr != "")
                        return parentStr;
                }
            }
            return "";
        }
        static int parentQtt = 0;
        static string searchInParent(string fileName, string path)
        {
            if (!path.EndsWith("cMake"))
            {
                var files = Directory.GetFiles(path);
                var file = files.Where(w => w.EndsWith(fileName)).FirstOrDefault();
                if (file != null)
                    return file;
                else
                {
                    parentQtt++;
                    var inChild = findInChildren(fileName, path);
                    if (inChild != "")
                        return inChild;
                    return searchInParent(fileName, path.Substring(0, path.LastIndexOf("\\")));
                }
            }
            return "";
        }

        static string findInChildren(string fileName, string path)
        {
            try
            {
                var files = Directory.GetFiles(path);
                var file = files.Where(w => w.EndsWith(fileName)).FirstOrDefault();
                if (file != null)
                    return file;
                else
                {
                    foreach (var directory in Directory.GetDirectories(path))
                    {
                        var inside = findInChildren(fileName, directory);
                        if (inside != "")
                            return inside;
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}
