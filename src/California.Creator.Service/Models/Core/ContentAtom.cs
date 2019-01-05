using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class ContentAtom // query filter => only !IsDeleted returned from DB // TODO make content atoms object oriented with inheritance?
    {
        public int ContentAtomId { get; set; } // TODO make the default content atoms uniqe for all users to save storage

        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }
        
        [Required]
        public ContentAtomType? ContentAtomType { get; set; }

        // type: text/html
        private string _textContent = null; // TODO private backing field!? need fluent setup??

        public string TextContent /*TODO use google to auto detect if content has been stolen TEST legal google use by company TEST legal google use by customer with customer data (indirect)*/ {
            get
            {
                return _textContent; 
            }
            set
            {
                _textContent = value;
                /*Console.WriteLine("original text:" + _textContent); TODO Hash TODO hash alternative key and TODO check same text if not => append counter to hash
                int unmanagedTextBufferSize = _textContent.Length * sizeof(char);
                IntPtr ptrToOriginalStringUnmanaged = System.Runtime.InteropServices.Marshal.StringToHGlobalUni(_textContent); // TODO check documentation if unicode char equals always 2 byte
                IntPtr ptrToCopiedStringUnmanaged = System.Runtime.InteropServices.Marshal.AllocHGlobal(unmanagedTextBufferSize + 1);
                byte[] preHashedData = new byte[unmanagedTextBufferSize];
                unsafe
                {
                    byte* src = (byte*)ptrToOriginalStringUnmanaged.ToPointer();
                    byte* dst = (byte*)ptrToCopiedStringUnmanaged.ToPointer();
                    for (int i = 0; i < unmanagedTextBufferSize; i++)
                    {
                        *dst++ = *src++;
                        preHashedData[i] = System.Runtime.InteropServices.Marshal.PtrToStructure<byte>(ptrToCopiedStringUnmanaged);
                    }
                    *dst = 0;
                }

                byte[] hashedBytes;
                try
                {
                    var hasher = System.Security.Cryptography.HashAlgorithm.Create(System.Security.Cryptography.HashAlgorithmName.MD5.ToString());
                    hashedBytes = hasher.ComputeHash(preHashedData);
                }
                catch (PlatformNotSupportedException ex)
                {
                    throw ex; // TODO alternative no hash!?
                }
                
                System.Runtime.InteropServices.Marshal.FreeHGlobal(ptrToCopiedStringUnmanaged);

                HashSha384 = hashedBytes;*/
            }
        } 
        //public byte[] HashSha384 { get; set; } = null;
        
        // type: picture
        public string Url { get; set; } = null; // TODO use google to auto detect if link contains malicious code // TODO detect emails
        public int? PictureContentId { get; set; }
        public PictureContent PictureContent { get; set; } // TODO use google to auto detect if content has been stolen // TODO hash TEST legal

        public bool IsDeleted { get; set; } = false; // query filter => !IsDeleted
        public DateTimeOffset? DeletedDate { get; set; } // TODO use tokyo tracker (1) client time stamp + time zone (2) display removed times and content

        [JsonIgnore]
        public DbContentSafetyLock DbContentSafetyLock { get; set; } = new DbContentSafetyLock(); // do not forget to add the lock
        
        public int? InstancedOnLayoutId { get; set; }
        [JsonIgnore]
        public LayoutAtom InstancedOnLayout { get; set; }
    }
}
