using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;

namespace dnpatch
{
    public class ResourcePatcher
    {
        private string file;
        private readonly ModuleDefMD module;
        private readonly ResourceCollection resources;

        public ResourcePatcher(string file)
        {
            this.file = file;
            module = ModuleDefMD.Load(file);
            resources = module.Resources;
        }

        public ResourceCollection GetResources()
        {
            return resources;
        }

        public void RemoveResource(int index)
        {
            resources.RemoveAt(index);
        }

        public void RemoveResources()
        {
            resources.Clear();
        }

        public void ReplaceResource(int index, string name, byte[] data)
        {
            resources[index] = new EmbeddedResource(name, data);
        }

        public void ReplaceResource(int index, string name, string file)
        {
            resources[index] = new EmbeddedResource(name, File.ReadAllBytes(file));
        }

        public void InsertResource(string name, byte[] data)
        {
            resources.Insert(resources.Count - 1, new EmbeddedResource(name, data));
        }

        public void InsertResource(string name, string file)
        {
            resources.Insert(resources.Count - 1, new EmbeddedResource(name, File.ReadAllBytes(file)));
        }

        public void Save(string name)
        {
            module.Write(name);
        }

        public void Save(bool backup)
        {
            module.Write(file + ".tmp");
            module.Dispose();
            if (backup)
            {
                if (File.Exists(file + ".bak"))
                {
                    File.Delete(file + ".bak");
                }
                File.Move(file, file + ".bak");
            }
            else
            {
                File.Delete(file);
            }
            File.Move(file + ".tmp", file);
        }
    }
}
