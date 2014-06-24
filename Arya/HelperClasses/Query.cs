﻿using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Windows.Forms;
using LinqKit;
using Arya.Framework.Data;
using Arya.Framework.Data.AryaDb;
using Attribute = Arya.Data.Attribute;
using EntityData = Arya.Data.EntityData;
using Group = Arya.Data.Group;
using SchemaData = Arya.Data.SchemaData;
using Sku = Arya.Data.Sku;
using TaxonomyInfo = Arya.Data.TaxonomyInfo;

//using System.Data.Objects.SqlClient;

namespace Arya.HelperClasses
{
    public static class Query
    {
        #region Methods (5)

        // Public Methods (5) 

        public static void DisplayCrossListInSkuView(CrossListCriteria cl)
        {
            if (cl != null)
            {
                var filteredSkus = GetFilteredSkus(
                    cl.TaxonomyIDFilter, cl.ValueFilters, cl.AttributeTypeFilters, cl.MatchAllTerms);

                if (filteredSkus == null)
                    MessageBox.Show("Crosslist not defined.");
                else
                    AryaTools.Instance.Forms.SkuForm.LoadTab(filteredSkus, null, "Query", "Query");
            }
            else
                MessageBox.Show("No Skus in crosslist node");
        }

        public static CrossListCriteria FetchCrossListObject(TaxonomyInfo taxInfo)
        {
            if (AryaTools.Instance.InstanceData.Dc.DerivedTaxonomies.Any(t => t.TaxonomyID == taxInfo.ID))
            {
                var cl = AryaTools.Instance.InstanceData.Dc.DerivedTaxonomies.Where(t => t.TaxonomyID == taxInfo.ID).Select(ex => ex.Expression).Single().DeSerializeXElement();
                cl.TaxonomyIDFilter = cl.TaxonomyIDFilter.GetTaxonomyIDs(cl.IncludeChildren); //Get SKUs in all the child Nodes to Display in SKU View  - Cross Ref
                return cl;
            }

            return null;
        }

        public static CrossListCriteria FetchCrossListObject(Group group)
        {
            if (group.Criterion != null)
            {
                var cl = AryaTools.Instance.InstanceData.Dc.Groups.Where(t => t.ID == group.ID).Select(ex => ex.Criterion).Single().DeSerializeXElement();
                cl.TaxonomyIDFilter = cl.TaxonomyIDFilter.GetTaxonomyIDs(cl.IncludeChildren); //Get SKUs in all the child Nodes to Display in SKU View  - Cross Ref
                return cl;
            }

            return null;

        }

        public static CrossListCriteria GetCrossListObject(TaxonomyInfo taxInfo)
        {
            if (AryaTools.Instance.InstanceData.Dc.DerivedTaxonomies.Any(t => t.TaxonomyID == taxInfo.ID))
            {
                var cl = AryaTools.Instance.InstanceData.Dc.DerivedTaxonomies.Where(t => t.TaxonomyID == taxInfo.ID).Select(ex => ex.Expression).Single().DeSerializeXElement();
                //cl.TaxonomyIDFilter = cl.TaxonomyIDFilter.GetTaxonomyIDs(cl.IncludeChildren); //Get SKUs in all the child Nodes to Display in SKU View  - Cross Ref
                return cl;
            }

            return null;
        }


        public static void BuildAndPredicates(List<ValueFilter> valueFilters, ref Expression<Func<Sku, bool>> skuPredicate, ref Expression<Func<Attribute, bool>> attributePredicate, ref Expression<Func<EntityData, bool>> entityDataPredicate)
        {
            for (int i = 0; i < valueFilters.Count; i++)
            {
                ValueFilter valueFilter = valueFilters[i];
                switch (valueFilter.FilterType)
                {
                    case "is in":
                        var parts =
                            valueFilter.Value.Split(
                                new[] { '\n', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(
                                    itemId => itemId.Trim()).Distinct().ToList();
                         skuPredicate = skuPredicate.And(result => parts.Contains(result.ItemID));
                        break;

                    case "is equal to":
                        if (valueFilter.Field.Equals("Item Id"))
                        {
                            skuPredicate = skuPredicate.And(result => result.ItemID.Equals(valueFilter.Value));
                        }
                        else if (valueFilter.Field.Equals("Value"))
                        {
                            entityDataPredicate = entityDataPredicate.And(result => result.Value.Equals(valueFilter.Value));
                        }
                        else if (valueFilter.Field.Equals("UoM"))
                        {
                            entityDataPredicate = entityDataPredicate.And(result => result.Uom.Equals(valueFilter.Value));
                        }
                        else if (valueFilter.Field.Equals("Attribute Name"))
                        {
                            attributePredicate = attributePredicate.And(result => result.AttributeName.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField1Name))
                        {
                            entityDataPredicate = entityDataPredicate.And(
                                result => result.Field1 != null && result.Field1.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField2Name))
                        {
                            entityDataPredicate = entityDataPredicate.And(
                                result => result.Field2 != null && result.Field2.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField3Name))
                        {
                            entityDataPredicate = entityDataPredicate.And(
                                result => result.Field3 != null && result.Field3.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField4Name))
                        {
                            entityDataPredicate = entityDataPredicate.And(
                                result => result.Field4 != null && result.Field4.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField5Name))
                        {
                            entityDataPredicate = entityDataPredicate.And(result => result.Field5.Equals(valueFilter.Value));
                        }
                        break;

                    case "contains":
                        if (valueFilter.Field.Equals("Value"))
                            entityDataPredicate = entityDataPredicate.And(result => result.Value.Contains(valueFilter.Value));
                        else if (valueFilter.Field.Equals("UoM"))
                            entityDataPredicate = entityDataPredicate.And(result => result.Uom != null && result.Uom.Contains(valueFilter.Value));
                        else if (valueFilter.Field.Equals("Attribute Name"))
                            attributePredicate = attributePredicate.And(result => result.AttributeName.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField1Name))
                            entityDataPredicate = entityDataPredicate.And(
                                result => result.Field1 != null && result.Field1.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField2Name))
                            entityDataPredicate = entityDataPredicate.And(
                                result => result.Field2 != null && result.Field2.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField3Name))
                            entityDataPredicate = entityDataPredicate.And(
                                result => result.Field3 != null && result.Field3.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField4Name))
                            entityDataPredicate = entityDataPredicate.And(
                                 result => result.Field4 != null && result.Field4.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField5Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field5 != null && result.Field5.Contains(valueFilter.Value));
                        break;

                    case "begins with":
                        if (valueFilter.Field.Equals("Value"))
                            entityDataPredicate = entityDataPredicate.And(result => result.Value.StartsWith(valueFilter.Value));
                        else if (valueFilter.Field.Equals("UoM"))
                            entityDataPredicate = entityDataPredicate.And(result => result.Uom != null && result.Uom.StartsWith(valueFilter.Value));
                        else if (valueFilter.Field.Equals("Attribute Name"))
                            attributePredicate = attributePredicate.And(
                                result => result.AttributeName.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField1Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field1 != null && result.Field1.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField2Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field2 != null && result.Field2.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField3Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field3 != null && result.Field3.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField4Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field4 != null && result.Field4.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField5Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field5 != null && result.Field5.StartsWith(valueFilter.Value));
                        break;

                    case "ends with":
                        if (valueFilter.Field.Equals("Value"))
                            entityDataPredicate = entityDataPredicate.And(result => result.Value.EndsWith(valueFilter.Value));
                        else if (valueFilter.Field.Equals("UoM"))
                            entityDataPredicate = entityDataPredicate.And(result => result.Uom != null && result.Uom.EndsWith(valueFilter.Value));
                        else if (valueFilter.Field.Equals("Attribute Name"))
                            attributePredicate = attributePredicate.And(result => result.AttributeName.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField1Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field1 != null && result.Field1.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField2Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field2 != null && result.Field2.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField3Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field3 != null && result.Field3.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField4Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field4 != null && result.Field4.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField5Name))
                            entityDataPredicate = entityDataPredicate.And(result => result.Field5 != null && result.Field5.EndsWith(valueFilter.Value));
                        break;
                }
            }
        }

        public static void BuildOrPredicates(List<ValueFilter> valueFilters, ref Expression<Func<Sku, bool>> skuPredicate,ref Expression<Func<Attribute, bool>> attributePredicate, ref Expression<Func<EntityData, bool>> entityDataPredicate)
        {
            for (int i = 0; i < valueFilters.Count; i++)
            {
                ValueFilter valueFilter = valueFilters[i];
                switch (valueFilter.FilterType)
                {
                    case "is in":
                        var parts =
                            valueFilter.Value.Split(
                                new[] { '\n', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(
                                    itemId => itemId.Trim()).Distinct().ToList();
                        skuPredicate = skuPredicate.Or(result => parts.Contains(result.ItemID));
                        break;

                    case "is equal to":
                        if (valueFilter.Field.Equals("Item Id"))
                        {
                            skuPredicate = skuPredicate.Or(result => result.ItemID.Equals(valueFilter.Value));
                        }
                        else if (valueFilter.Field.Equals("Value"))
                        {
                            entityDataPredicate = entityDataPredicate.Or(result => result.Value.Equals(valueFilter.Value));
                        }
                        else if (valueFilter.Field.Equals("UoM"))
                        {
                            entityDataPredicate = entityDataPredicate.Or(result => result.Uom.Equals(valueFilter.Value));
                        }
                        else if (valueFilter.Field.Equals("Attribute Name"))
                        {
                            attributePredicate = attributePredicate.Or(result => result.AttributeName.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField1Name))
                        {
                            entityDataPredicate = entityDataPredicate.Or(
                                result => result.Field1 != null && result.Field1.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField2Name))
                        {
                            entityDataPredicate = entityDataPredicate.Or(
                                result => result.Field2 != null && result.Field2.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField3Name))
                        {
                            entityDataPredicate = entityDataPredicate.Or(
                                result => result.Field3 != null && result.Field3.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField4Name))
                        {
                            entityDataPredicate = entityDataPredicate.Or(
                                result => result.Field4 != null && result.Field4.Equals(valueFilter.Value));
                        }
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField5Name))
                        {
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field5.Equals(valueFilter.Value));
                        }
                        break;

                    case "contains":
                        if (valueFilter.Field.Equals("Value"))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Value.Contains(valueFilter.Value));
                        else if (valueFilter.Field.Equals("UoM"))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Uom != null && result.Uom.Contains(valueFilter.Value));
                        else if (valueFilter.Field.Equals("Attribute Name"))
                            attributePredicate = attributePredicate.Or(result => result.AttributeName.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField1Name))
                            entityDataPredicate = entityDataPredicate.Or(
                                result => result.Field1 != null && result.Field1.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField2Name))
                            entityDataPredicate = entityDataPredicate.Or(
                                result => result.Field2 != null && result.Field2.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField3Name))
                            entityDataPredicate = entityDataPredicate.Or(
                                result => result.Field3 != null && result.Field3.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField4Name))
                            entityDataPredicate = entityDataPredicate.Or(
                                 result => result.Field4 != null && result.Field4.Contains(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField5Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field5 != null && result.Field5.Contains(valueFilter.Value));
                        break;

                    case "begins with":
                        if (valueFilter.Field.Equals("Value"))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Value.StartsWith(valueFilter.Value));
                        else if (valueFilter.Field.Equals("UoM"))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Uom != null && result.Uom.StartsWith(valueFilter.Value));
                        else if (valueFilter.Field.Equals("Attribute Name"))
                            attributePredicate = attributePredicate.Or(
                                result => result.AttributeName.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField1Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field1 != null && result.Field1.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField2Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field2 != null && result.Field2.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField3Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field3 != null && result.Field3.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField4Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field4 != null && result.Field4.StartsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField5Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field5 != null && result.Field5.StartsWith(valueFilter.Value));
                        break;

                    case "ends with":
                        if (valueFilter.Field.Equals("Value"))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Value.EndsWith(valueFilter.Value));
                        else if (valueFilter.Field.Equals("UoM"))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Uom != null && result.Uom.EndsWith(valueFilter.Value));
                        else if (valueFilter.Field.Equals("Attribute Name"))
                            attributePredicate = attributePredicate.Or(result => result.AttributeName.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField1Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field1 != null && result.Field1.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField2Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field2 != null && result.Field2.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField3Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field3 != null && result.Field3.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField4Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field4 != null && result.Field4.EndsWith(valueFilter.Value));
                        else if (
                            valueFilter.Field.Equals(
                                AryaTools.Instance.InstanceData.CurrentProject.EntityField5Name))
                            entityDataPredicate = entityDataPredicate.Or(result => result.Field5 != null && result.Field5.EndsWith(valueFilter.Value));
                        break;
                }
            }
        }

        public static IQueryable<Sku> GetFilteredSkus(List<Guid> taxonomyIDFilter, List<ValueFilter> valueFilters, HashSet<string> attributeTypeFilters, bool matchAllTerms)
        {
            var baseQueryFiltered = false;

            //All Taxonomy Related Predicates.
            bool anyTaxnomyPredicates = true;
            var taxonomyPredicate = PredicateBuilder.True<TaxonomyInfo>();

            var taxExclusions = AryaTools.Instance.InstanceData.CurrentUser.TaxonomyExclusions.ToList();
            Expression<Func<TaxonomyInfo, bool>> taxExclusionsPredicate = result => true;

            if(taxExclusions.Count > 0)
                taxExclusionsPredicate = result => !taxExclusions.Contains(result.ID);
                
            if ((taxonomyIDFilter == null || taxonomyIDFilter.Count == 0) && taxExclusions.Count == 0)
            {
                anyTaxnomyPredicates = false;
            }

            if (taxonomyIDFilter != null && taxonomyIDFilter.Count > 0)
            {
                baseQueryFiltered = true;
                taxonomyPredicate = result => taxonomyIDFilter.Contains(result.ID);
            }


            //All Attribute Related Predicates
            var attributePredicate = matchAllTerms
                                         ? PredicateBuilder.True<Attribute>()
                                         : PredicateBuilder.False<Attribute>();
            var attributeExclusions = AryaTools.Instance.InstanceData.CurrentUser.AttributeExclusions.ToList();
            Expression<Func<Attribute, bool>> attributeExclusionsPredicate = result => true;

            if(attributeExclusions.Count > 0)
                attributeExclusionsPredicate = result => !attributeExclusions.Contains(result.ID);

            var attributeGlobalPredicate = PredicateBuilder.True<Attribute>();

            //All Schema Related Predicates
            var schemaPredicate = PredicateBuilder.True<SchemaData>();
            bool anySchemaPredicates = false;

            //All Sku Related Predicates
            var skuPredicate = matchAllTerms ? PredicateBuilder.True<Sku>() : PredicateBuilder.False<Sku>();
            var entityDataPredicate = matchAllTerms
                                          ? PredicateBuilder.True<EntityData>()
                                          : PredicateBuilder.False<EntityData>();

            var skuExclusions = AryaTools.Instance.InstanceData.CurrentUser.SkuExclusions.ToList();
            Expression<Func<Sku, bool>> skuExclusionsPredicate = result => true;
            if(skuExclusions.Count > 0)
                skuExclusionsPredicate = result => !skuExclusions.Contains(result.ID);

            if (attributeTypeFilters.Count > 0)
            {
                foreach (var attributeTypeFilter in attributeTypeFilters)
                {
                    switch (attributeTypeFilter)
                    {
                        case "Nav":
                            schemaPredicate.And(result => result.NavigationOrder > 0);
                            anySchemaPredicates = true;
                            break;

                        case "Disp":
                            schemaPredicate.And(result => result.DisplayOrder > 0);
                            anySchemaPredicates = true;
                            break;

                        case "Global":
                            attributeGlobalPredicate.And(
                                result => result.Type.Equals(AttributeTypeEnum.Global));
                            break;

                        case "InSchema":
                            schemaPredicate.And(result => result.InSchema);
                            anySchemaPredicates = true;
                            break;
                    }
                }
            }

            if (valueFilters != null && valueFilters.Count > 0 && valueFilters[0].Value != "(2k items max)")
            {
                baseQueryFiltered = true;
                if (matchAllTerms) BuildAndPredicates(valueFilters, ref skuPredicate, ref attributePredicate, ref entityDataPredicate);
                else
                {
                    BuildOrPredicates(valueFilters, ref skuPredicate, ref attributePredicate, ref entityDataPredicate);
                }
            }

            //This is plain stupid, but works!!
            if (skuPredicate.ToString() == PredicateBuilder.False<Sku>().ToString())
            {
                skuPredicate = PredicateBuilder.True<Sku>();
            }
            if (attributePredicate.ToString() == PredicateBuilder.False<Attribute>().ToString())
            {
                attributePredicate = PredicateBuilder.True<Attribute>();
            }
            if (entityDataPredicate.ToString() == PredicateBuilder.False<EntityData>().ToString())
            {
                entityDataPredicate = PredicateBuilder.True<EntityData>();
            }

            IQueryable<Sku> finalQuery;

            if (anySchemaPredicates && anyTaxnomyPredicates)
            {
                finalQuery =
                    (from ed in AryaTools.Instance.InstanceData.Dc.EntityDatas.Where(p => p.Active).Where(entityDataPredicate)
                    join a in AryaTools.Instance.InstanceData.Dc.Attributes.Where(attributeGlobalPredicate).Where(attributeExclusionsPredicate).Where(attributePredicate) on ed.AttributeID equals
                        a.ID
                    join ei in AryaTools.Instance.InstanceData.Dc.EntityInfos on ed.EntityID equals ei.ID
                    join s in
                        AryaTools.Instance.InstanceData.Dc.Skus.Where(p => p.ProjectID == AryaTools.Instance.InstanceData.CurrentProject.ID).Where(skuExclusionsPredicate)
                        .Where(skuPredicate) on ei.SkuID equals s.ID
                    join si in AryaTools.Instance.InstanceData.Dc.SkuInfos.Where(p => p.Active) on s.ID equals si.SkuID
                    join t in AryaTools.Instance.InstanceData.Dc.TaxonomyInfos.Where(taxExclusionsPredicate).Where(taxonomyPredicate) on si.TaxonomyID equals
                        t.ID
                    join sci in AryaTools.Instance.InstanceData.Dc.SchemaInfos on new { TaxonomyID = t.ID, AttributeID = a.ID }
                        equals new { sci.TaxonomyID, sci.AttributeID }
                    join scd in AryaTools.Instance.InstanceData.Dc.SchemaDatas.Where(p => p.Active).Where(schemaPredicate) on
                        sci.ID equals scd.SchemaID
                    select s).Distinct();
            }
            else if (anySchemaPredicates)
            {
                finalQuery =
                    (from ed in AryaTools.Instance.InstanceData.Dc.EntityDatas.Where(p => p.Active).Where(entityDataPredicate)
                    join a in AryaTools.Instance.InstanceData.Dc.Attributes.Where(attributeGlobalPredicate).Where(attributeExclusionsPredicate).Where(attributePredicate) on ed.AttributeID equals
                        a.ID
                    join ei in AryaTools.Instance.InstanceData.Dc.EntityInfos on ed.EntityID equals ei.ID
                    join s in
                        AryaTools.Instance.InstanceData.Dc.Skus.Where(p => p.ProjectID == AryaTools.Instance.InstanceData.CurrentProject.ID).Where(skuExclusionsPredicate)
                        .Where(skuPredicate) on ei.SkuID equals s.ID
                    join si in AryaTools.Instance.InstanceData.Dc.SkuInfos.Where(p => p.Active) on s.ID equals si.SkuID
                    join sci in AryaTools.Instance.InstanceData.Dc.SchemaInfos on new { si.TaxonomyID, AttributeID = a.ID }
                        equals new { sci.TaxonomyID, sci.AttributeID }
                    join scd in AryaTools.Instance.InstanceData.Dc.SchemaDatas.Where(p => p.Active).Where(schemaPredicate) on
                        sci.ID equals scd.SchemaID
                    select s).Distinct();
            }
            else if (anyTaxnomyPredicates)
            {
                finalQuery =
                    (from ed in AryaTools.Instance.InstanceData.Dc.EntityDatas.Where(p => p.Active).Where(entityDataPredicate)
                    join a in AryaTools.Instance.InstanceData.Dc.Attributes.Where(attributeGlobalPredicate).Where(attributeExclusionsPredicate).Where(attributePredicate) on ed.AttributeID equals
                        a.ID
                    join ei in AryaTools.Instance.InstanceData.Dc.EntityInfos on ed.EntityID equals ei.ID
                    join s in
                        AryaTools.Instance.InstanceData.Dc.Skus.Where(p => p.ProjectID == AryaTools.Instance.InstanceData.CurrentProject.ID).Where(skuExclusionsPredicate)
                        .Where(skuPredicate) on ei.SkuID equals s.ID
                    join si in AryaTools.Instance.InstanceData.Dc.SkuInfos.Where(p => p.Active) on s.ID equals si.SkuID
                    join t in AryaTools.Instance.InstanceData.Dc.TaxonomyInfos.Where(taxExclusionsPredicate).Where(taxonomyPredicate) on si.TaxonomyID equals
                        t.ID
                    select s).Distinct();
            }
            else
            {
                finalQuery =
                  (from ed in AryaTools.Instance.InstanceData.Dc.EntityDatas.Where(p => p.Active).Where(entityDataPredicate)
                  join a in AryaTools.Instance.InstanceData.Dc.Attributes.Where(attributeGlobalPredicate).Where(attributeExclusionsPredicate).Where(attributePredicate) on ed.AttributeID equals
                      a.ID
                  join ei in AryaTools.Instance.InstanceData.Dc.EntityInfos on ed.EntityID equals ei.ID
                  join s in
                      AryaTools.Instance.InstanceData.Dc.Skus.Where(p => p.ProjectID == AryaTools.Instance.InstanceData.CurrentProject.ID).Where(skuExclusionsPredicate)
                      .Where(skuPredicate) on ei.SkuID equals s.ID
                  select s).Distinct();
            }

            finalQuery = finalQuery.Where(s => s.SkuType == Framework.Data.AryaDb.Sku.ItemType.Product.ToString());
            return baseQueryFiltered ? finalQuery : null;
        }

        public static List<Guid> GetTaxonomyIDs(this IEnumerable<Guid> listOfTaxonomies, bool includeChildrenforCrossList)
        {
            var taxonomyFilters = AryaTools.Instance.InstanceData.Dc.TaxonomyInfos.Where(t => listOfTaxonomies.Contains(t.ID)).ToList();
            if (taxonomyFilters.Count <= 0)
                return null;
            var taxIds = taxonomyFilters.Select(tf => tf.ID).ToList();
            if (includeChildrenforCrossList)
            {
                taxonomyFilters.ForEach(tax => taxIds.AddRange(tax.AllChildren.Select(t => t.ID)));
            }

            if (taxIds.Count <= 2000)
                return taxIds;

            MessageBox.Show("Too many nodes in taxonomy filter.");

            return null;
        }

        public static List<Guid> GetTaxonomyIDs(this IEnumerable<TaxonomyInfo> listOfTaxonomyInfos, bool includeChildrenforCrossList)
        {
            return listOfTaxonomyInfos == null ? null : listOfTaxonomyInfos.Select(tf => tf.ID).ToList().GetTaxonomyIDs(includeChildrenforCrossList);
        }

        #endregion Methods
    }
    public class QueryResult
    {
        #region Properties (7)

        public Guid AttributeId { get; set; }

        public decimal DispOrder { get; set; }

        public EntityData EntityData { get; set; }

        public bool InSchema { get; set; }

        public bool IsGlobal { get; set; }

        public decimal NavOrder { get; set; }

        public Sku Sku { get; set; }

        public Guid TaxonomyId { get; set; }

        #endregion Properties
    }
    public class SearchResult
    {
        #region Properties (2)

        public int SkuCount { get; set; }

        public TaxonomyInfo Taxonomy { get; set; }

        #endregion Properties
    }

    [Serializable]
    public class ValueFilter : ISerializable
    {
        #region Properties (3)

        public string Field { get; set; }

        public string FilterType { get; set; }

        public string Value { get; set; }

        public string Tolerance { get; set; }

        #endregion Properties

        #region constructor

        public ValueFilter()
        {
        }

        public ValueFilter(SerializationInfo info, StreamingContext ctxt)
        {
            this.Field = (string)info.GetValue("Field", typeof(string));
            this.FilterType = (string)info.GetValue("FilterType", typeof(string));
            this.Value = (string)info.GetValue("Value", typeof(string));
            this.Value = (string)info.GetValue("Tolerance", typeof(string));
        }

        #endregion constructor

        #region method
        void ISerializable.GetObjectData(SerializationInfo oInfo, StreamingContext oContext)
        {
            oInfo.AddValue("Field", this.Field);
            oInfo.AddValue("FilterType", this.FilterType);
            oInfo.AddValue("Value", this.Value);
            oInfo.AddValue("Tolerance   ", this.Value);
        }

        #endregion methods
    }
}