// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace NymaTypes
{

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct NAxisInfo : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static NAxisInfo GetRootAsNAxisInfo(ByteBuffer _bb) { return GetRootAsNAxisInfo(_bb, new NAxisInfo()); }
  public static NAxisInfo GetRootAsNAxisInfo(ByteBuffer _bb, NAxisInfo obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public NAxisInfo __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public string SettingsNameNeg { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetSettingsNameNegBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
  public ArraySegment<byte>? GetSettingsNameNegBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public byte[] GetSettingsNameNegArray() { return __p.__vector_as_array<byte>(4); }
  public string SettingsNamePos { get { int o = __p.__offset(6); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetSettingsNamePosBytes() { return __p.__vector_as_span<byte>(6, 1); }
#else
  public ArraySegment<byte>? GetSettingsNamePosBytes() { return __p.__vector_as_arraysegment(6); }
#endif
  public byte[] GetSettingsNamePosArray() { return __p.__vector_as_array<byte>(6); }
  public string NameNeg { get { int o = __p.__offset(8); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameNegBytes() { return __p.__vector_as_span<byte>(8, 1); }
#else
  public ArraySegment<byte>? GetNameNegBytes() { return __p.__vector_as_arraysegment(8); }
#endif
  public byte[] GetNameNegArray() { return __p.__vector_as_array<byte>(8); }
  public string NamePos { get { int o = __p.__offset(10); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNamePosBytes() { return __p.__vector_as_span<byte>(10, 1); }
#else
  public ArraySegment<byte>? GetNamePosBytes() { return __p.__vector_as_arraysegment(10); }
#endif
  public byte[] GetNamePosArray() { return __p.__vector_as_array<byte>(10); }

  public static Offset<NymaTypes.NAxisInfo> CreateNAxisInfo(FlatBufferBuilder builder,
      StringOffset SettingsNameNegOffset = default(StringOffset),
      StringOffset SettingsNamePosOffset = default(StringOffset),
      StringOffset NameNegOffset = default(StringOffset),
      StringOffset NamePosOffset = default(StringOffset)) {
    builder.StartTable(4);
    NAxisInfo.AddNamePos(builder, NamePosOffset);
    NAxisInfo.AddNameNeg(builder, NameNegOffset);
    NAxisInfo.AddSettingsNamePos(builder, SettingsNamePosOffset);
    NAxisInfo.AddSettingsNameNeg(builder, SettingsNameNegOffset);
    return NAxisInfo.EndNAxisInfo(builder);
  }

  public static void StartNAxisInfo(FlatBufferBuilder builder) { builder.StartTable(4); }
  public static void AddSettingsNameNeg(FlatBufferBuilder builder, StringOffset SettingsNameNegOffset) { builder.AddOffset(0, SettingsNameNegOffset.Value, 0); }
  public static void AddSettingsNamePos(FlatBufferBuilder builder, StringOffset SettingsNamePosOffset) { builder.AddOffset(1, SettingsNamePosOffset.Value, 0); }
  public static void AddNameNeg(FlatBufferBuilder builder, StringOffset NameNegOffset) { builder.AddOffset(2, NameNegOffset.Value, 0); }
  public static void AddNamePos(FlatBufferBuilder builder, StringOffset NamePosOffset) { builder.AddOffset(3, NamePosOffset.Value, 0); }
  public static Offset<NymaTypes.NAxisInfo> EndNAxisInfo(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<NymaTypes.NAxisInfo>(o);
  }
  public NAxisInfoT UnPack() {
    var _o = new NAxisInfoT();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(NAxisInfoT _o) {
    _o.SettingsNameNeg = this.SettingsNameNeg;
    _o.SettingsNamePos = this.SettingsNamePos;
    _o.NameNeg = this.NameNeg;
    _o.NamePos = this.NamePos;
  }
  public static Offset<NymaTypes.NAxisInfo> Pack(FlatBufferBuilder builder, NAxisInfoT _o) {
    if (_o == null) return default(Offset<NymaTypes.NAxisInfo>);
    var _SettingsNameNeg = _o.SettingsNameNeg == null ? default(StringOffset) : builder.CreateString(_o.SettingsNameNeg);
    var _SettingsNamePos = _o.SettingsNamePos == null ? default(StringOffset) : builder.CreateString(_o.SettingsNamePos);
    var _NameNeg = _o.NameNeg == null ? default(StringOffset) : builder.CreateString(_o.NameNeg);
    var _NamePos = _o.NamePos == null ? default(StringOffset) : builder.CreateString(_o.NamePos);
    return CreateNAxisInfo(
      builder,
      _SettingsNameNeg,
      _SettingsNamePos,
      _NameNeg,
      _NamePos);
  }
};

public class NAxisInfoT
{
  public string SettingsNameNeg { get; set; }
  public string SettingsNamePos { get; set; }
  public string NameNeg { get; set; }
  public string NamePos { get; set; }

  public NAxisInfoT() {
    this.SettingsNameNeg = null;
    this.SettingsNamePos = null;
    this.NameNeg = null;
    this.NamePos = null;
  }
}


}