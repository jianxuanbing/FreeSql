﻿using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql.Odbc.KingbaseES
{

    class OdbcKingbaseESUpdate<T1> : Internal.CommonProvider.UpdateProvider<T1> where T1 : class
    {

        public OdbcKingbaseESUpdate(IFreeSql orm, CommonUtils commonUtils, CommonExpression commonExpression, object dywhere)
            : base(orm, commonUtils, commonExpression, dywhere)
        {
        }

        internal string InternalTableAlias { get; set; }
        internal StringBuilder InternalSbSet => _set;
        internal StringBuilder InternalSbSetIncr => _setIncr;
        internal Dictionary<string, bool> InternalIgnore => _ignore;
        internal void InternalResetSource(List<T1> source) => _source = source;
        internal string InternalWhereCaseSource(string CsName, Func<string, string> thenValue) => WhereCaseSource(CsName, thenValue);
        internal void InternalToSqlCaseWhenEnd(StringBuilder sb, ColumnInfo col) => ToSqlCaseWhenEnd(sb, col);

        public override int ExecuteAffrows() => base.SplitExecuteAffrows(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override List<T1> ExecuteUpdated() => base.SplitExecuteUpdated(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        protected override List<T1> RawExecuteUpdated()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.CsType, col.Attribute.MapType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
            sql = sb.ToString();
            var dbParms = _params.Concat(_paramsSource).ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = _orm.Ado.Query<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, sql, dbParms);
                ValidateVersionAndThrow(ret.Count);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }

        protected override void ToSqlCase(StringBuilder caseWhen, ColumnInfo[] primarys)
        {
            if (_table.Primarys.Length == 1)
            {
                var pk = _table.Primarys.First();
                if (string.IsNullOrEmpty(InternalTableAlias) == false) caseWhen.Append(InternalTableAlias).Append(".");
                caseWhen.Append(_commonUtils.QuoteReadColumn(pk.CsType, pk.Attribute.MapType, _commonUtils.QuoteSqlName(pk.Attribute.Name)));
                return;
            }
            caseWhen.Append("(");
            var pkidx = 0;
            foreach (var pk in _table.Primarys)
            {
                if (pkidx > 0) caseWhen.Append(" || '+' || ");
                if (string.IsNullOrEmpty(InternalTableAlias) == false) caseWhen.Append(InternalTableAlias).Append(".");
                caseWhen.Append(_commonUtils.QuoteReadColumn(pk.CsType, pk.Attribute.MapType, _commonUtils.QuoteSqlName(pk.Attribute.Name))).Append("::text");
                ++pkidx;
            }
            caseWhen.Append(")");
        }

        protected override void ToSqlWhen(StringBuilder sb, ColumnInfo[] primarys, object d)
        {
            if (_table.Primarys.Length == 1)
            {
                sb.Append(_commonUtils.FormatSql("{0}", _table.Primarys.First().GetMapValue(d)));
                return;
            }
            sb.Append("(");
            var pkidx = 0;
            foreach (var pk in _table.Primarys)
            {
                if (pkidx > 0) sb.Append(" || '+' || ");
                sb.Append(_commonUtils.FormatSql("{0}", pk.GetMapValue(d))).Append("::text");
                ++pkidx;
            }
            sb.Append(")");
        }

        protected override void ToSqlCaseWhenEnd(StringBuilder sb, ColumnInfo col)
        {
            if (_noneParameter == false) return;
            if (col.Attribute.MapType == typeof(string))
            {
                sb.Append("::text");
                return;
            }
            var dbtype = _commonUtils.CodeFirst.GetDbInfo(col.Attribute.MapType)?.dbtype;
            if (dbtype == null) return;

            sb.Append("::").Append(dbtype);
        }

#if net40
#else
        public override Task<int> ExecuteAffrowsAsync() => base.SplitExecuteAffrowsAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);
        public override Task<List<T1>> ExecuteUpdatedAsync() => base.SplitExecuteUpdatedAsync(_batchRowsLimit > 0 ? _batchRowsLimit : 500, _batchParameterLimit > 0 ? _batchParameterLimit : 3000);

        async protected override Task<List<T1>> RawExecuteUpdatedAsync()
        {
            var sql = this.ToSql();
            if (string.IsNullOrEmpty(sql)) return new List<T1>();

            var sb = new StringBuilder();
            sb.Append(sql).Append(" RETURNING ");

            var colidx = 0;
            foreach (var col in _table.Columns.Values)
            {
                if (colidx > 0) sb.Append(", ");
                sb.Append(_commonUtils.QuoteReadColumn(col.CsType, col.Attribute.MapType, _commonUtils.QuoteSqlName(col.Attribute.Name))).Append(" as ").Append(_commonUtils.QuoteSqlName(col.CsName));
                ++colidx;
            }
            sql = sb.ToString();
            var dbParms = _params.Concat(_paramsSource).ToArray();
            var before = new Aop.CurdBeforeEventArgs(_table.Type, _table, Aop.CurdType.Update, sql, dbParms);
            _orm.Aop.CurdBeforeHandler?.Invoke(this, before);
            var ret = new List<T1>();
            Exception exception = null;
            try
            {
                ret = await _orm.Ado.QueryAsync<T1>(_table.TypeLazy ?? _table.Type, _connection, _transaction, CommandType.Text, sql, dbParms);
                ValidateVersionAndThrow(ret.Count);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw ex;
            }
            finally
            {
                var after = new Aop.CurdAfterEventArgs(before, exception, ret);
                _orm.Aop.CurdAfterHandler?.Invoke(this, after);
            }
            return ret;
        }
#endif
    }
}
