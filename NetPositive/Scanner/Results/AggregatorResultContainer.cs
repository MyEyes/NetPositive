using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AggregatorNet;

namespace NetPositive.Scanner.Results
{
    internal class AggregatorResultContainer : IResultContainer
    {
        // Token: 0x06000041 RID: 65 RVA: 0x000034C0 File Offset: 0x000016C0
        public AggregatorResultContainer(string aggregatorUri, string[] args)
        {
            string[] split = aggregatorUri.Split('@', StringSplitOptions.None);
            string aggregatorCredentials = split[0];
            string[] usersplit = aggregatorCredentials.Split(':', StringSplitOptions.None);
            this.aggregator = new Aggregator(split[1].Trim(), usersplit[0], usersplit[1]);
            this.tool = this.aggregator.CreateTool("NetPositive", "Tool for mass scanning .Net assembly for signatures", "1.0");
            this.scan = this.aggregator.StartScan(this.tool, HashHelper.GetSHA256String("NetPositive" + DateTime.Now.ToString()), HashHelper.GetSHA256String("NetPositive" + DateTime.Now.ToString()), string.Join(" ", args).Replace(aggregatorCredentials, "<censored>"));
            this.PathProperty = this.aggregator.CreatePropertyKind("Path", "Path of a file subject", false);
            this.FunctionNameProperty = this.aggregator.CreatePropertyKind("FunctionName", "Name of a function", true);
            this.FileNameProperty = this.aggregator.CreatePropertyKind("FileName", "Name of a file without full path", true);
            this.VersionProperty = this.aggregator.CreatePropertyKind("Version", "A Version identifier", true);
            this._tagCache.Add("open", new Tag(this.aggregator, "open", "lightblue", "open", "Not yet reviewed"));
            this._tagCache.Add("closed", new Tag(this.aggregator, "closed", "darkgray", "closed", "Reviewed"));
            this._tagCache.Add("confirmed", new Tag(this.aggregator, "confirmed", "darkgreen", "confirmed", "Confirmed real finding"));
            this._tagCache.Add("rejected", new Tag(this.aggregator, "rejected", "darkred", "rejected", "False positive - rejected"));
        }

        // Token: 0x06000042 RID: 66 RVA: 0x000036D4 File Offset: 0x000018D4
        ~AggregatorResultContainer()
        {
            bool flag = this.scan != null;
            if (flag)
            {
                this.aggregator.StopScan(this.scan);
            }
            this.aggregator.FinishQueue();
        }

        // Token: 0x06000043 RID: 67 RVA: 0x0000372C File Offset: 0x0000192C
        public string GenerateResultMarkdown(ScanResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("``` csharp");
            sb.AppendLine(result.Method.FullName);
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine(result.Description);
            return sb.ToString();
        }

        // Token: 0x06000044 RID: 68 RVA: 0x00003788 File Offset: 0x00001988
        public void AddResult(ScanResult result)
        {
            string path = result.Method.Module.Location;
            Subject subj = this.CreateOrGetSubject(result);
            string file_hash = subj.hash;
            List<Property> resultProperties = new List<Property>
            {
                this.FileNameProperty.Create(Path.GetFileName(path)),
                this.FunctionNameProperty.Create(result.Method.FullName)
            };
            List<Tag> resultTags = new List<Tag>();
            foreach (string tag_name in result.Tags)
            {
                resultTags.Add(this.CreateOrGetTag(tag_name, "gray", "", ""));
            }
            resultTags.Add(this.CreateOrGetTag("open", "gray", "", ""));
            this.aggregator.SubmitResult(new Result(this.aggregator, this.scan, subj, file_hash + ":" + result.Method.FullName, result.Risk, result.Title, this.GenerateResultMarkdown(result), resultProperties, resultTags));
        }

        // Token: 0x06000045 RID: 69 RVA: 0x000038C8 File Offset: 0x00001AC8
        private Subject CreateOrGetSubject(ScanResult result)
        {
            string path = result.Method.Module.Location;
            bool flag = this._subjectCache.ContainsKey(path);
            Subject subject;
            if (flag)
            {
                subject = this._subjectCache[path];
            }
            else
            {
                string file_hash = "";
                try
                {
                    string assemblyHash = Convert.ToHexString(result.Method.Module.Assembly.ToAssemblyRef().Hash);
                    file_hash = assemblyHash;
                }
                catch
                {
                    file_hash = HashHelper.GetSHA256StringFromFile(path);
                }
                List<Property> subjectProperties = new List<Property>
                {
                    this.FileNameProperty.Create(Path.GetFileName(path)),
                    this.PathProperty.Create(path),
                    this.VersionProperty.Create(result.Method.Module.Assembly.Version.ToString())
                };
                Subject subj = this.aggregator.CreateSubject(Path.GetFileName(path), file_hash, subjectProperties, "1.0", null);
                this._subjectCache.Add(path, subj);
                subject = subj;
            }
            return subject;
        }

        // Token: 0x06000046 RID: 70 RVA: 0x000039E4 File Offset: 0x00001BE4
        private Tag CreateOrGetTag(string tag, string color = "gray", string name = "", string description = "")
        {
            bool flag = this._tagCache.ContainsKey(tag);
            Tag tag2;
            if (flag)
            {
                tag2 = this._tagCache[tag];
            }
            else
            {
                bool flag2 = name.Length == 0;
                if (flag2)
                {
                    name = tag;
                }
                Tag new_tag = new Tag(this.aggregator, tag, color, name, description);
                this._tagCache.Add(tag, new_tag);
                tag2 = new_tag;
            }
            return tag2;
        }

        // Token: 0x06000047 RID: 71 RVA: 0x00003A45 File Offset: 0x00001C45
        public void Flush()
        {
            this.aggregator.StopScan(this.scan);
            this.scan = null;
            this.aggregator.FinishQueue();
        }

        // Token: 0x0400001D RID: 29
        private Aggregator aggregator;

        // Token: 0x0400001E RID: 30
        private Tool tool;

        // Token: 0x0400001F RID: 31
        private Scan scan;

        // Token: 0x04000020 RID: 32
        private PropertyKind PathProperty;

        // Token: 0x04000021 RID: 33
        private PropertyKind FileNameProperty;

        // Token: 0x04000022 RID: 34
        private PropertyKind VersionProperty;

        // Token: 0x04000023 RID: 35
        private PropertyKind FunctionNameProperty;

        // Token: 0x04000024 RID: 36
        private Dictionary<string, Subject> _subjectCache = new Dictionary<string, Subject>();

        // Token: 0x04000025 RID: 37
        private Dictionary<string, Tag> _tagCache = new Dictionary<string, Tag>();
    }
}
