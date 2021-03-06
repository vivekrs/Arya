using System;
using System.Collections.Generic;
using System.Linq;
using Arya.Framework.Common.Extensions;
using Arya.Framework.Extensions;

namespace Arya.Framework.Data.AryaDb
{
    public partial class Group : BaseEntity
    {
        public const string SKU_GROUP_UD = "Sku_UD";
        public const string USER_GROUP_UD = "User_UD";
        public const string USER_GROUP_PD = "User_PD";
        public static readonly Guid DefaultGroupID = new Guid("8A37744E-EFFA-4A1A-8EE1-E5BAF09925A4");

        //Predefined UserGroups
        public static readonly Guid RoleManagerGroup = new Guid("C70DCA7A-6077-47A6-85EB-8740B1C6D6AB");
        public static readonly Guid PermissionsManagerGroup = new Guid("D045B1E7-4E64-4340-91FD-BDE8C4706D3C");
        public static readonly Guid ReadOnlyGroup = new Guid("0C529A01-7E75-4F19-8816-278A2941E705");
        public static readonly Guid ImportAdminGroup = new Guid("DF71F6AC-14E2-42E5-8DA5-1906CCA7362F");
        public static readonly Guid ImportUserGroup = new Guid("75A60D43-397E-4E15-8822-B151A2829F31");
        public static readonly Guid ExportAdminGroup = new Guid("9B8B2929-6D68-4822-B1F3-074243B2209A");
        public static readonly Guid StandardExportUserGroup = new Guid("1CB39D6B-D23E-4B79-BDB4-329F54BA7347");
        public static readonly Guid CustomExportUserGroup = new Guid("9346DBCC-4C70-443E-8AC7-C005518EBFF5");

        public Group(AryaDbDataContext parentContext, bool initialize = true) : this()
        {
            ParentContext = parentContext;
            Initialize = initialize;
            InitEntity();
        }

        public HashSet<Guid> TaxonomyExlusions
        {
            get
            {
                if (GroupType == SKU_GROUP_UD)
                    throw new InvalidOperationException("Property not valid for Sku Groups");

                return
                    Roles.Where(
                        r => r.Permission == false && r.ObjectType == Role.TaskObjectType.TaxonomyInfo.ToString())
                        .Select(p => p.ObjectID)
                        .ToList()
                        .ToHashSet();
            }
        }

        public HashSet<Guid> AttributeExlusions
        {
            get
            {
                if (GroupType == SKU_GROUP_UD)
                    throw new InvalidOperationException("Property not valid for Sku Groups");

                return
                    Roles.Where(r => r.Permission == false && r.ObjectType == Role.TaskObjectType.Attribute.ToString())
                        .Select(p => p.ObjectID)
                        .ToList()
                        .ToHashSet();
            }
        }

        public HashSet<Guid> SkuExlusions
        {
            get
            {
                if (GroupType == SKU_GROUP_UD)
                    throw new InvalidOperationException("Property not valid for Sku Groups");

                return
                    Roles.Where(r => r.Permission == false && r.ObjectType == Role.TaskObjectType.Sku.ToString())
                        .Select(p => p.ObjectID)
                        .ToList()
                        .ToHashSet();
            }
        }

        public HashSet<Guid> UIExclusions
        {
            get
            {
                if (GroupType == SKU_GROUP_UD)
                    throw new InvalidOperationException("Property not valid for Sku Groups");

                return
                    Roles.Where(r => r.Permission == false && r.ObjectType == Role.TaskObjectType.UIObject.ToString())
                        .Select(p => p.ObjectID)
                        .ToList()
                        .ToHashSet();
            }
        }

        public List<Sku> this[TaxonomyInfo index]
        {
            get
            {
                return
                    SkuGroups.SelectMany(s => s.Sku.SkuInfos.Where(a => a.Active))
                        .Where(t => t.TaxonomyInfo == index)
                        .Select(s => s.Sku)
                        .ToList();
            }
        }

        public List<Sku> Skus
        {
            get { return SkuGroups.Where(a => a.Active).Select(s => s.Sku).ToList(); }
        }

        public List<TaxonomyInfo> Nodes
        {
            get
            {
                return
                    SkuGroups.SelectMany(s => s.Sku.SkuInfos.Where(a => a.Active))
                        .Select(t => t.TaxonomyInfo)
                        .Distinct()
                        .ToList();
            }
        }

        public int SkuCount
        {
            get { return SkuGroups.Count(a => a.Active); }
        }
    }
}
