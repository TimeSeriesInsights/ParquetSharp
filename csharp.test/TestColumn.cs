using System;
using ParquetSharp.Schema;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ParquetSharp.Test
{
    [TestFixture]
    internal static class TestColumn
    {
        [Test]
        public static void TestPrimitives()
        {
            var expectedPrimitives = CreateExpectedPrimitives().ToList();

            foreach (var unicodeString in UnicodeCharactersGenerator.GetCommonUsedUnicodeCharacterSets())
            {
                expectedPrimitives.Add(new ExpectedPrimitive
                {
                    Type = typeof(bool),
                    PhysicalType = PhysicalType.Boolean,
                    Name = unicodeString
                });
            }

            expectedPrimitives.Add(new ExpectedPrimitive
            {
                Type = typeof(bool),
                PhysicalType = PhysicalType.Boolean,
                Name = UnicodeCharactersGenerator.GetAllAsciiCharacters()
            });

            foreach (var expected in expectedPrimitives)
            {
                Console.WriteLine("Testing primitive type {0}", expected.Type);

                Assert.True(Column.IsSupported(expected.Type));

                var type = expected.Type;
                var isDecimal = type == typeof(decimal) || type == typeof(decimal?);
                var column = new Column(type, expected.Name, expected.LogicalTypeOverride);

                using (var node = column.CreateSchemaNode())
                {
                    Assert.AreEqual(expected.LogicalType, node.LogicalType);
                    Assert.AreEqual(-1, node.Id);
                    Assert.AreEqual(expected.Name, node.Name);
                    Assert.AreEqual(NodeType.Primitive, node.NodeType);
                    Assert.AreEqual(null, node.Parent);
                    Assert.AreEqual(expected.Repetition, node.Repetition);

                    var primitive = (PrimitiveNode) node;

                    Assert.AreEqual(expected.ColumnOrder, primitive.ColumnOrder);
                    Assert.AreEqual(expected.PhysicalType, primitive.PhysicalType);
                    Assert.AreEqual(expected.Length, primitive.TypeLength);
                    Assert.AreEqual(expected.LogicalType, primitive.LogicalType);
                }
            }
        }

        [Test]
        public static void TestUnsupportedType()
        {
            Assert.False(Column.IsSupported(typeof(TestColumn)));

            var exception = Assert.Throws<ArgumentException>(() => new Column<object>("unsupported").CreateSchemaNode());
            Assert.AreEqual("unsupported logical type System.Object", exception.Message);
        }

        [Test]
        public static void TestUnsupportedLogicalTypeOverride()
        {
            var exception = Assert.Throws<ParquetException>(() => 
                new Column<DateTime>("DateTime", LogicalType.Json()).CreateSchemaNode());

            Assert.That(
                exception.Message,
                Contains.Substring("JSON can not be applied to primitive type INT64"));
        }

        private static ExpectedPrimitive[] CreateExpectedPrimitives()
        {
            return new[]
            {
                new ExpectedPrimitive
                {
                    Type = typeof(bool),
                    PhysicalType = PhysicalType.Boolean
                },
                new ExpectedPrimitive
                {
                    Type = typeof(sbyte),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(8, isSigned: true)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(byte),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(8, isSigned: false)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(short),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(16, isSigned: true)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(ushort),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(16, isSigned: false)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(int),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(32, isSigned: true)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(uint),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(32, isSigned: false)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(long),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Int(64, isSigned: true)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(ulong),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Int(64, isSigned: false)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(float),
                    PhysicalType = PhysicalType.Float
                },
                new ExpectedPrimitive
                {
                    Type = typeof(double),
                    PhysicalType = PhysicalType.Double
                },
                new ExpectedPrimitive
                {
                    Type = typeof(decimal),
                    PhysicalType = PhysicalType.FixedLenByteArray,
                    LogicalType = LogicalType.Decimal(29, 3),
                    LogicalTypeOverride = LogicalType.Decimal(29, 3),
                    Length = 16
                },
                new ExpectedPrimitive
                {
                    Type = typeof(Date),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Date()
                },
                new ExpectedPrimitive
                {
                    Type = typeof(DateTime),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Timestamp(true, TimeUnit.Micros)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(DateTime),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Timestamp(true, TimeUnit.Millis),
                    LogicalTypeOverride = LogicalType.Timestamp(true, TimeUnit.Millis)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(DateTimeNanos),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Timestamp(true, TimeUnit.Nanos)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(TimeSpan),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Time(true, TimeUnit.Micros)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(TimeSpan),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Time(true, TimeUnit.Millis),
                    LogicalTypeOverride = LogicalType.Time(true, TimeUnit.Millis)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(TimeSpanNanos),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Time(true, TimeUnit.Nanos)
                },
                new ExpectedPrimitive
                {
                    Type = typeof(string),
                    PhysicalType = PhysicalType.ByteArray,
                    LogicalType = LogicalType.String(),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(string),
                    PhysicalType = PhysicalType.ByteArray,
                    LogicalType = LogicalType.Json(),
                    LogicalTypeOverride = LogicalType.Json(),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(byte[]),
                    PhysicalType = PhysicalType.ByteArray,
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(byte[]),
                    PhysicalType = PhysicalType.ByteArray,
                    LogicalType = LogicalType.Bson(),
                    LogicalTypeOverride = LogicalType.Bson(),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(bool?),
                    PhysicalType = PhysicalType.Boolean,
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(sbyte?),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(8, isSigned: true),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(byte?),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(8, isSigned: false),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(short?),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(16, isSigned: true),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(ushort?),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(16, isSigned: false),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(int?),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(32, isSigned: true),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(uint?),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Int(32, isSigned: false),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(long?),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Int(64, isSigned: true),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(ulong?),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Int(64, isSigned: false),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(float?),
                    PhysicalType = PhysicalType.Float,
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(double?),
                    PhysicalType = PhysicalType.Double,
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(decimal?),
                    PhysicalType = PhysicalType.FixedLenByteArray,
                    LogicalType = LogicalType.Decimal(29, 2),
                    LogicalTypeOverride = LogicalType.Decimal(29, 2),
                    Repetition = Repetition.Optional,
                    Length = 16
                },
                new ExpectedPrimitive
                {
                    Type = typeof(Date?),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Date(),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(DateTime?),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Timestamp(true, TimeUnit.Micros),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(DateTime?),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Timestamp(true, TimeUnit.Millis),
                    LogicalTypeOverride = LogicalType.Timestamp(true, TimeUnit.Millis),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(DateTimeNanos?),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Timestamp(true, TimeUnit.Nanos),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(TimeSpan?),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Time(true, TimeUnit.Micros),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(TimeSpan?),
                    PhysicalType = PhysicalType.Int32,
                    LogicalType = LogicalType.Time(true, TimeUnit.Millis),
                    LogicalTypeOverride = LogicalType.Time(true, TimeUnit.Millis),
                    Repetition = Repetition.Optional
                },
                new ExpectedPrimitive
                {
                    Type = typeof(TimeSpanNanos?),
                    PhysicalType = PhysicalType.Int64,
                    LogicalType = LogicalType.Time(true, TimeUnit.Nanos),
                    Repetition = Repetition.Optional
                }
            };
        }

        private sealed class ExpectedPrimitive
        {
            public Type Type;
            public LogicalType LogicalType = LogicalType.None();
            public LogicalType LogicalTypeOverride = LogicalType.None();
            public string Name = "MyName";
            public Repetition Repetition = Repetition.Required;
            public ColumnOrder ColumnOrder = ColumnOrder.TypeDefinedOrder;
            public PhysicalType PhysicalType;
            public int Length = -1;
        }

        public static class UnicodeCharactersGenerator
        {
            /// <summary>
            /// http://www.cl.cam.ac.uk/~mgk25/ucs/examples/quickbrown.txt
            /// </summary>
            private static readonly string CommonlyUsedUnicodeCharacters = @"
Quizdeltagerne spiste jordbær med fløde, mens cirkusklovnen  Wolther spillede på xylofon
Falsches Üben von Xylophonmusik quält jeden größeren Zwerg
Zwölf Boxkämpfer jagten Eva quer über den Sylter Deich
Heizölrückstoßabdämpfung
Γαζέες καὶ μυρτιὲς δὲν θὰ βρῶ πιὰ στὸ χρυσαφὶ ξέφωτο
Ξεσκεπάζω τὴν ψυχοφθόρα βδελυγμία
The quick brown fox jumps over the lazy dog
El pingüino Wenceslao hizo kilómetros bajo exhaustiva lluvia y frío, añoraba a su querido cachorro
Portez ce vieux whisky au juge blond qui fume sur son île intérieure, à côté de l'alcôve ovoïde, où les bûches se consument dans l'âtre, ce qui lui permet de penser à la cænogenèse de l'être dont il est question dans la cause ambiguë entendue à Moÿ, dans un capharnaüm qui, pense-t-il, diminue çà et là la qualité de son œuvre. l'île exiguë Où l'obèse jury mûr Fête l'haï volapük, Âne ex aéquo au whist, Ôtez ce vœu déçu. Le cœur déçu mais l'âme plutôt naïve, Louÿs rêva de crapaüter en canoë au delà des îles, près du mälström où brûlent les novæ
D'fhuascail Íosa, Úrmhac na hÓighe Beannaithe, pór Éava agus Ádhaimh
Árvíztűrő tükörfúrógép
Kæmi ný öxi hér ykist þjófum nú bæði víl og ádrepa Sævör grét áðan því úlpan var ónýt
いろはにほへとちりぬるを  わかよたれそつねならむ  うゐのおくやまけふこえて  あさきゆめみしゑひもせす
イロハニホヘト チリヌルヲ ワカヨタレソ ツネナラム  ウヰノオクヤマ ケフコエテ アサキユメミシ ヱヒモセスン
? דג סקרן שט בים מאוכזב ולפתע מצא לו חברה איך הקליטה
Pchnąć w tę łódź jeża lub ośm skrzyń fig
В чащах юга жил бы цитрус? Да, но фальшивый экземпляр!
Съешь же ещё этих мягких французских булок да выпей чаю
๏ เป็นมนุษย์สุดประเสริฐเลิศคุณค่า  กว่าบรรดาฝูงสัตว์เดรัจฉาน  จงฝ่าฟันพัฒนาวิชาการ  อย่าล้างผลาญฤๅเข่นฆ่าบีฑาใคร  ไม่ถือโทษโกรธแช่งซัดฮึดฮัดด่าหัดอภัยเหมือนกีฬาอัชฌาสัย  ปฏิบัติประพฤติกฎกำหนดใจ  พูดจาให้จ๊ะๆ จ๋าๆ น่าฟังเอย ฯ
Pijamalı hasta, yağız şoföre çabucak güvendi
தெய்வத்தான் ஆகா தெனினும் முயற்சிதன் மெய்வருத்தக் கூலி தரும்";

            public static IEnumerable<string> GetCommonUsedUnicodeCharacterSets()
            {
                foreach (string characterSet in CommonlyUsedUnicodeCharacters.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return characterSet;
                }
                yield return GetAllAsciiCharacters();
            }

            public static string GetAllAsciiCharacters()
            {
                StringBuilder allAsciiCharacters = new StringBuilder(capacity: 128, maxCapacity: 128);
                for (int i = 0; i < 128; i++)
                {
                    allAsciiCharacters.Append((char)i);
                }
                return allAsciiCharacters.ToString();
            }
        }
    }
}
