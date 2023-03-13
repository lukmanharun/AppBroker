using Magicodes.ExporterAndImporter.Core.Extension;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Core.Filters;
using Magicodes.ExporterAndImporter.Core.Models;
using Magicodes.ExporterAndImporter.Excel.Utility;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.IE.Core;
using SixLabors.ImageSharp;

namespace AppBroker.Services.Magicode
{
    public class MagicodeIEHelper
    {
        private ExcelImporterAttribute _excelImporterAttribute;
        /// <summary>
        ///     列头定义
        /// </summary>
        protected List<ImporterHeaderInfo> ImporterHeaderInfos { get; set; }
        internal ImportResult<T> ImportResult { get; set; }
        /// <summary>
        /// 导出业务错误数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">流</param>
        /// <param name="bussinessErrorDataList">错误数据</param>
        /// <param name="fileByte">成功:错误数据返回文件流字节,失败 返回null</param>
        /// <returns></returns>
        public virtual bool OutputBussinessErrorData<T>(Stream stream, List<DataRowErrorInfo> bussinessErrorDataList, out byte[] fileByte) where T : class, new()
        {
            using (var importer = new ImportHelper<T>())
            {
                return importer.OutputBussinessErrorDataByte(stream, bussinessErrorDataList, out fileByte);
            }
        }
        /// <summary>
        /// 将存在的错误数据通过导入模板返回,并且标识业务错误原因
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="bussinessErrorDataList">错误的业务数据</param>
        /// <param name="fileByte">成功:错误错误文件流字节,失败 返回null</param>
        /// <returns></returns>
        public bool OutputBussinessErrorDataByte(Stream stream, List<DataRowErrorInfo> bussinessErrorDataList, out byte[] fileByte)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    using (var excelPackage = new ExcelPackage(memoryStream))
                    {
                        //生成Excel错误标注
                        LabelingBussinessError(excelPackage, bussinessErrorDataList, out fileByte);
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                fileByte = null;
                return false;
            }
        }

        /// <summary>
        /// 标注业务错误
        /// </summary>
        /// <param name="excelPackage"></param>
        /// <param name="bussinessErrorDataList"></param>
        /// <param name="fileByte">返回错误Excel流字节</param>
        internal virtual void LabelingBussinessError(ExcelPackage excelPackage,
            List<DataRowErrorInfo> bussinessErrorDataList, out byte[] fileByte)
        {
            if (bussinessErrorDataList == null)
            {
                fileByte = null;
                return;
            }

            this.ImportResult = new ImportResult<T>();
            ParseHeader();
            ParseTemplate(excelPackage);
            //执行结果筛选器
            var filter = GetFilter<IImportResultFilter>(ExcelImporterSettings.ImportResultFilter);
            if (filter != null)
            {
                ImportResult = filter.Filter(ImportResult);
            }

            //if (ExcelImporterSettings.IsLabelingError && ImportResult.HasError)
            //业务错误必须标注
            var worksheet = GetImportSheet(excelPackage);

            //标注数据错误
            foreach (var item in bussinessErrorDataList)
            {
                //item.RowIndex += ExcelImporterSettings.HeaderRowIndex;
                foreach (var field in item.FieldErrors)
                {
                    var col = ImporterHeaderInfos.First(p => p.Header.Name == field.Key);
                    var cell = worksheet.Cells[item.RowIndex, col.Header.ColumnIndex];
                    cell.Style.Font.Color.SetColor(Color.Red);
                    cell.Style.Font.Bold = true;
                    if (cell.Comment == null)
                    {
                        cell.AddComment(string.Join(",", field.Value), col.Header.Author);
                    }
                    else
                    {
                        cell.Comment.Text = field.Value;
                    }
                }
            }

            using (var stream = new MemoryStream())
            {
                excelPackage.SaveAs(stream);
                fileByte = stream.ToArray();
            }
        }


        /// <summary>
        ///     解析头部
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException">导入实体没有定义ImporterHeader属性</exception>
        protected virtual bool ParseHeader()
        {
            ImporterHeaderInfos = new List<ImporterHeaderInfo>();
            // var objProperties = typeof(T).GetProperties();
            var objProperties = typeof(T).GetSortedPropertyInfos();

            if (objProperties.Length == 0) return false;

            foreach (var propertyInfo in objProperties)
            {
                //TODO:简化并重构
                //如果不设置，则自动使用默认定义
                var importerHeaderAttribute =
                    (propertyInfo.GetCustomAttributes(typeof(ImporterHeaderAttribute), true) as
                        ImporterHeaderAttribute[])?.FirstOrDefault() ?? new ImporterHeaderAttribute
                        {
                            Name = propertyInfo.GetDisplayName() ?? propertyInfo.Name
                        };

                if (string.IsNullOrWhiteSpace(importerHeaderAttribute.Name))
                    importerHeaderAttribute.Name = propertyInfo.GetDisplayName() ?? propertyInfo.Name;
                var ignore = (propertyInfo.GetAttribute<IEIgnoreAttribute>(true) == null)
                    ? importerHeaderAttribute.IsIgnore
                    : propertyInfo.GetAttribute<IEIgnoreAttribute>(true).IsImportIgnore;
                //忽略字段处理
                if (ignore) continue;

                var colHeader = new ImporterHeaderInfo
                {
                    IsRequired = propertyInfo.IsRequired(),
                    PropertyName = propertyInfo.Name,
                    Header = importerHeaderAttribute,
                    ImportImageFieldAttribute = propertyInfo.GetAttribute<ImportImageFieldAttribute>(true),
                    PropertyInfo = propertyInfo
                };

                //设置ColumnIndex
                if (colHeader.Header.ColumnIndex > 0)
                {
                    colHeader.Header.ColumnIndex = colHeader.Header.ColumnIndex;
                }
                else if (propertyInfo.GetAttribute<DisplayAttribute>(true) != null &&
                         propertyInfo.GetAttribute<DisplayAttribute>(true).GetOrder() != null)
                {
                    colHeader.Header.ColumnIndex = propertyInfo.GetAttribute<DisplayAttribute>(true).Order;
                }

                //设置Description
                if (colHeader.Header.Description.IsNullOrWhiteSpace())
                {
                    if (propertyInfo.GetAttribute<DescriptionAttribute>()?.Description != null)
                    {
                        colHeader.Header.Description = propertyInfo.GetAttribute<DescriptionAttribute>()?.Description;
                    }
                    else if (propertyInfo.GetAttribute<DisplayAttribute>()?.Description != null)
                    {
                        colHeader.Header.Description = propertyInfo.GetAttribute<DisplayAttribute>()?.Description;
                    }
                }

                colHeader.Header.IsIgnore = ignore;

                ImporterHeaderInfos.Add(colHeader);

                #region 处理值映射

                var colHeaderMappingValues = colHeader.MappingValues;
                propertyInfo.ValueMapping(ref colHeaderMappingValues);
                #endregion 处理值映射
            }

            #region 执行列筛选器

            var filter = GetFilter<IImportHeaderFilter>(ExcelImporterSettings.ImportHeaderFilter);
            if (filter != null)
            {
                ImporterHeaderInfos = filter.Filter(ImporterHeaderInfos);
            }

            #endregion 执行列筛选器

            return true;
        }

        /// <summary>
        ///     获取导入的Sheet
        /// </summary>
        /// <param name="excelPackage"></param>
        /// <returns></returns>
        protected virtual ExcelWorksheet GetImportSheet(ExcelPackage excelPackage)
        {
            return excelPackage.Workbook.Worksheets[typeof(T).GetDisplayName()] ??
                               excelPackage.Workbook.Worksheets[ExcelImporterSettings.SheetName] ??
                               excelPackage.Workbook.Worksheets[0];
        }
        /// <summary>
        ///     解析模板
        /// </summary>
        /// <returns></returns>
        protected virtual void ParseTemplate(ExcelPackage excelPackage)
        {
            ImportResult.TemplateErrors = new List<TemplateErrorInfo>();
            try
            {
                //根据名称获取Sheet，如果不存在则取第一个
                var worksheet = GetImportSheet(excelPackage);
                var excelHeaders = new Dictionary<string, int>();
                var endColumnCount = ExcelImporterSettings.EndColumnCount ?? worksheet.Dimension.End.Column;
                if (!string.IsNullOrWhiteSpace(ExcelImporterSettings.ImportDescription))
                {
                    ExcelImporterSettings.HeaderRowIndex++;
                }

                for (var columnIndex = 1; columnIndex <= endColumnCount; columnIndex++)
                {
                    var header = worksheet.Cells[ExcelImporterSettings.HeaderRowIndex, columnIndex].Text;

                    //如果未设置读取的截止列，则默认指定为出现空格，则读取截止
                    if (ExcelImporterSettings.EndColumnCount.HasValue &&
                        columnIndex > ExcelImporterSettings.EndColumnCount.Value ||
                        string.IsNullOrWhiteSpace(header))
                        break;

                    //不处理空表头
                    if (string.IsNullOrWhiteSpace(header)) continue;

                    if (excelHeaders.ContainsKey(header))
                        ImportResult.TemplateErrors.Add(new TemplateErrorInfo
                        {
                            ErrorLevel = ErrorLevels.Error,
                            ColumnName = header,
                            RequireColumnName = null,
                            Message = Resource.ColumnHeadRepeat
                        });

                    excelHeaders.Add(header, columnIndex);
                }

                foreach (var item in ImporterHeaderInfos)
                {
                    //支持忽略列名的大小写
                    var isColumnExist = false;
                    if (ExcelImporterSettings.IsIgnoreColumnCase)
                    {
                        var excelHeaderName = (excelHeaders.Keys.FirstOrDefault(p => p.Equals(item.Header.Name, StringComparison.CurrentCultureIgnoreCase)));
                        isColumnExist = excelHeaderName != null;
                        if (isColumnExist)
                        {
                            item.Header.Name = excelHeaderName;
                        }
                    }
                    else
                    {
                        isColumnExist = excelHeaders.ContainsKey(item.Header.Name);
                    }
                    if (!isColumnExist)
                    {
                        if (item.Header.ColumnIndex > 0)
                        {
                            item.IsExist = true;
                            continue;
                        }

                        //仅验证必填字段
                        if (item.IsRequired)
                        {
                            ImportResult.TemplateErrors.Add(new TemplateErrorInfo
                            {
                                ErrorLevel = ErrorLevels.Error,
                                ColumnName = null,
                                RequireColumnName = item.Header.Name,
                                Message = Resource.ImportTemplateNotFoundThisField
                            });
                            continue;
                        }

                        ImportResult.TemplateErrors.Add(new TemplateErrorInfo
                        {
                            ErrorLevel = ErrorLevels.Warning,
                            ColumnName = null,
                            RequireColumnName = item.Header.Name,
                            Message = Resource.ImportTemplateNotFoundThisField
                        });
                    }
                    else
                    {
                        item.IsExist = true;
                        //设置列索引
                        if (item.Header.ColumnIndex == 0)
                            item.Header.ColumnIndex = excelHeaders[item.Header.Name];
                    }
                }
            }
            catch (Exception ex)
            {
                ImportResult.TemplateErrors.Add(new TemplateErrorInfo
                {
                    ErrorLevel = ErrorLevels.Error,
                    ColumnName = null,
                    RequireColumnName = null,
                    Message = $"{Resource.AnUnknownErrorOccurredInTheTemplate}{ex}"
                });
                throw new Exception($"{Resource.AnUnknownErrorOccurredInTheTemplate}{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取筛选器
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        /// <param name="filterType"></param>
        /// <returns></returns>
        private TFilter GetFilter<TFilter>(Type filterType = null) where TFilter : IFilter
        {
            return filterType.GetFilter<TFilter>(ExcelImporterSettings.IsDisableAllFilter);
        }

        /// <summary>
        ///     导入全局设置
        /// </summary>
        protected ExcelImporterAttribute ExcelImporterSettings
        {
            get
            {
                if (_excelImporterAttribute == null)
                {
                    var type = typeof(T);
                    _excelImporterAttribute = type.GetAttribute<ExcelImporterAttribute>(true);
                    if (_excelImporterAttribute != null) return _excelImporterAttribute;

                    var importerAttribute = type.GetAttribute<ImporterAttribute>(true);
                    if (importerAttribute != null)
                    {
                        _excelImporterAttribute = new ExcelImporterAttribute()
                        {
                            HeaderRowIndex = importerAttribute.HeaderRowIndex,
                            MaxCount = importerAttribute.MaxCount,
                            ImportResultFilter = importerAttribute.ImportResultFilter,
                            ImportHeaderFilter = importerAttribute.ImportHeaderFilter,
                            IsDisableAllFilter = importerAttribute.IsDisableAllFilter,
                            IsIgnoreColumnCase = importerAttribute.IsIgnoreColumnCase
                        };
                    }
                    else
                        _excelImporterAttribute = new ExcelImporterAttribute();

                    return _excelImporterAttribute;
                }

                return _excelImporterAttribute;
            }
            set => _excelImporterAttribute = value;
        }
    }
}
