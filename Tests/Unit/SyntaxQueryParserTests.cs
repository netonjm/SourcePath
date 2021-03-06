using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AshMind.Extensions;
using Xunit;
using SourcePath.CSharp;

namespace SourcePath.Tests.Unit {
    public class SyntaxQueryParserTests {
        [Theory]
        [InlineData("if", SyntaxQueryKeyword.If)]
        public void Parse_Basic(string queryAsString, SyntaxQueryKeyword expectedTarget) {
            var query = new SyntaxQueryParser().Parse(queryAsString);
            Assert.Equal(expectedTarget, query.Keyword);
        }

        [Theory]
        [InlineData("if", SyntaxQueryAxis.Child)]
        [InlineData("/if", SyntaxQueryAxis.Child)]
        [InlineData("//if", SyntaxQueryAxis.Descendant)]
        [InlineData("descendant::if", SyntaxQueryAxis.Descendant)]
        [InlineData("self::if", SyntaxQueryAxis.Self)]
        [InlineData("parent::if", SyntaxQueryAxis.Parent)]
        public void Parse_Axis(string queryAsString, SyntaxQueryAxis expectedAxis) {
            var query = new SyntaxQueryParser().Parse(queryAsString);
            Assert.Equal(expectedAxis, query.Axis);
        }

        [Theory]
        [InlineData("if[if]")]
        [InlineData("if[if && if]")]
        [InlineData("if[if && if && if]")]
        [InlineData("if[if && if[if && if]]")]
        [InlineData("class[name == 'C']")]
        [InlineData("class[name == 'C' && method[name == 'M']]")]
        public void Parse_Filter(string queryAsString) {
            var query = new SyntaxQueryParser().Parse(queryAsString);
            Assert.Equal(queryAsString, query.ToString());
        }

        [Theory]
        [MemberData(nameof(GetAllKeywords))]
        public void Parse_Keyword(string keyword) {
            // Assert.DoesNotThrow
            new SyntaxQueryParser().Parse(keyword);
        }

        public static IEnumerable<object[]> GetAllKeywords() {
            var source = GetRoslynKeywordsSourceAsync().Result;
            var keywords = SyntaxFactory.ParseCompilationUnit(source)
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.Text != "GetPreprocessorKeywordKind" && m.Identifier.Text != "GetText")
                .SelectMany(m => m.Body.DescendantNodes())
                .OfType<LiteralExpressionSyntax>()
                .Select(s => s.Token.Value)
                .OfType<string>()
                .Where(k => Regex.IsMatch(k, "^[a-z]+$"));
            return keywords.Select(k => new string[] { k });
        }

        private static async Task<string> GetRoslynKeywordsSourceAsync() {
            var factsPath = Path.Combine(
                Assembly.GetExecutingAssembly().GetAssemblyFileFromCodeBase().DirectoryName,
                "roslyn_SyntaxKindFacts.cached.cs"
            );
            if (File.Exists(factsPath))
                return await File.ReadAllTextAsync(factsPath);

            using (var client = new HttpClient()) {
                var facts = await client.GetStringAsync(
                    "https://raw.githubusercontent.com/dotnet/roslyn/1b0cf5c732062f66b71a3d62a165d6eb5f8b3022/src/Compilers/CSharp/Portable/Syntax/SyntaxKindFacts.cs"
                ).ConfigureAwait(false);
                await File.WriteAllTextAsync(factsPath, facts).ConfigureAwait(false);
                return facts;
            }
        }
    }
}
