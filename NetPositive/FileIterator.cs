using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace NetPositive
{
    internal class FileIterator : IEnumerable<string>, IEnumerable
    {

        private string[] basePaths;
        private string[] resolvedBasePaths;
        private bool recursive;
        private bool allowPathEscape;
        public FileIterator(string[] basePaths, bool recursive, bool allowPathEscape)
        {
            this.basePaths = basePaths;
            this.recursive = recursive;
            this.allowPathEscape = allowPathEscape;
            this.resolvedBasePaths = new string[this.basePaths.Length];
            for (int i = 0; i < this.resolvedBasePaths.Length; i++)
            {
                this.resolvedBasePaths[i] = NativeMethods.GetFinalPathName(this.basePaths[i]);
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x0000255C File Offset: 0x0000075C
        public void Print()
        {
            Console.WriteLine("Running on the following paths:");
            foreach (string basePath in this.basePaths)
            {
                Console.Write("\t- ");
                Console.WriteLine(basePath);
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x000025A4 File Offset: 0x000007A4
        private void _EnqueueChildren(string path, Queue<string> queue, HashSet<string> done)
        {
            try
            {
                string[] subDirs = Directory.GetDirectories(path);
                for (int i = 0; i < subDirs.Length; i++)
                {
                    try
                    {
                        string newPath = subDirs[i];
                        string newAbsPath = Path.GetFullPath(subDirs[i]);
                        string newResolvedPath = NativeMethods.GetFinalPathName(newAbsPath);
                        bool flag = !done.Contains(newPath) && !done.Contains(newAbsPath) && !done.Contains(newResolvedPath);
                        if (flag)
                        {
                            bool flag2 = this.allowPathEscape || this.isBelowBasePath(newResolvedPath);
                            if (flag2)
                            {
                                queue.Enqueue(newPath);
                            }
                            done.Add(newPath);
                            done.Add(newAbsPath);
                            done.Add(newResolvedPath);
                        }
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch (Win32Exception)
            {
            }
            catch (UnauthorizedAccessException uae)
            {
            }
            catch (Exception e2)
            {
                Console.WriteLine(string.Format("Couldn't iterate directories in {0}. {1}", path, e2));
            }
            try
            {
                string[] files = Directory.GetFiles(path);
                for (int j = 0; j < files.Length; j++)
                {
                    try
                    {
                        string newPath2 = files[j];
                        string newAbsPath2 = Path.GetFullPath(files[j]);
                        string newResolvedPath2 = NativeMethods.GetFinalPathName(newAbsPath2);
                        bool flag3 = !done.Contains(newPath2) && !done.Contains(newAbsPath2) && !done.Contains(newResolvedPath2);
                        if (flag3)
                        {
                            bool flag4 = this.allowPathEscape || this.isBelowBasePath(newResolvedPath2);
                            if (flag4)
                            {
                                queue.Enqueue(newPath2);
                            }
                            done.Add(newPath2);
                            done.Add(newAbsPath2);
                            done.Add(newResolvedPath2);
                        }
                    }
                    catch (Exception e3)
                    {
                    }
                }
            }
            catch (Win32Exception)
            {
            }
            catch (UnauthorizedAccessException uae2)
            {
            }
            catch (Exception e4)
            {
                Console.WriteLine(string.Format("Couldn't iterate files in {0}. {1}", path, e4));
            }
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000027C4 File Offset: 0x000009C4
        private bool isBelowBasePath(string resolvedPath)
        {
            for (int i = 0; i < this.resolvedBasePaths.Length; i++)
            {
                bool flag = resolvedPath.StartsWith(this.resolvedBasePaths[i]);
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00002808 File Offset: 0x00000A08
        public bool MatchesFilter(string path)
        {
            string ext = Path.GetExtension(path);
            return ext == ".exe" || ext == ".dll";
        }

        // Token: 0x06000013 RID: 19 RVA: 0x0000283C File Offset: 0x00000A3C
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            Queue<string> pendingPaths = new Queue<string>();
            HashSet<string> traversedPaths = new HashSet<string>();
            foreach (string p in this.basePaths)
            {
                pendingPaths.Enqueue(p);
            }
            string[] array = null;
            while (pendingPaths.Count > 0)
            {
                string current = pendingPaths.Dequeue();
                bool flag = this.recursive && Directory.Exists(current);
                if (flag)
                {
                    this._EnqueueChildren(current, pendingPaths, traversedPaths);
                }
                bool flag2 = File.Exists(current) && this.MatchesFilter(current);
                if (flag2)
                {
                    yield return current;
                }
                current = null;
            }
            yield break;
        }

        // Token: 0x06000014 RID: 20 RVA: 0x0000284C File Offset: 0x00000A4C
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

    }
}
