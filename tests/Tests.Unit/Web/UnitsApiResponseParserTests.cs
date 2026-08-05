using System.Net;
using System.Text;
using FluentAssertions;
using SmartCondo.Web.Services;

namespace Tests.Unit.Web;

public sealed class UnitsApiResponseParserTests
{
    [Fact]
    public async Task ParseAsync_WithProblemDetails_Should_ReturnValidationMessage()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                """
                {
                  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                  "title": "One or more validation errors occurred.",
                  "status": 400,
                  "errors": {
                    "Papel": ["The JSON value could not be converted to PapelVinculo."]
                  }
                }
                """,
                Encoding.UTF8,
                "application/problem+json")
        };

        var result = await UnitsApiResponseParser.ParseAsync<UnitCreatedModel>(response);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("PapelVinculo");
        result.Message.Should().NotContain("conectar");
    }

    [Fact]
    public async Task ParseAsync_WithUnexpectedJson_Should_NotThrowKeyNotFoundException()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("""{"traceId":"abc-123"}""", Encoding.UTF8, "application/json")
        };

        var action = () => UnitsApiResponseParser.ParseAsync<UnitCreatedModel>(response);

        var result = await action.Should().NotThrowAsync();
        result.Subject.IsSuccess.Should().BeFalse();
        result.Subject.StatusCode.Should().Be(500);
        result.Subject.Message.Should().Contain("resposta inesperada");
    }

    [Fact]
    public async Task ParseAsync_WithResultEnvelope_Should_DeserializeData()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(
                """
                {
                  "isSuccess": true,
                  "message": "Unidade cadastrada",
                  "errors": [],
                  "data": { "unitId": 10, "residentId": 20, "vinculoId": 30 }
                }
                """,
                Encoding.UTF8,
                "application/json")
        };

        var result = await UnitsApiResponseParser.ParseAsync<UnitCreatedModel>(response);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data!.UnitId.Should().Be(10);
        result.Data.ResidentId.Should().Be(20);
    }
}
