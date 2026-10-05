using System;

namespace BMW.Rheingold.CoreFramework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AuthorAPIAttribute : Attribute
    {
        private bool selectableTypeDeclaration;
        public bool SelectableTypeDeclaration
        {
            get
            {
                return selectableTypeDeclaration;
            }

            set
            {
                selectableTypeDeclaration = value;
            }
        }
    }
}