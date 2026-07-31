using WppSender.Domain;
using Xunit;

namespace WppSender.Domain.Tests;

public class UsuarioTests
{
    private class HasherFakeQueSempreConfirma : IPasswordHasher
    {
        public string Hash(string senhaPlana) => "hash-qualquer";
        public bool Verify(string senhaPlana, string hash) => true;
    }

    private class HasherFakeQueSempreNega : IPasswordHasher
    {
        public string Hash(string senhaPlana) => "hash-qualquer";
        public bool Verify(string senhaPlana, string hash) => false;
    }

    [Fact]
    public void DeveAutenticar_QuandoSenhaEstaCorreta()
    {
        var usuario = new Usuario(Guid.NewGuid(), "user@teste.com", "hash-armazenado");

        var resultado = usuario.Autenticar("senha-correta", new HasherFakeQueSempreConfirma());

        Assert.True(resultado);
    }

    [Fact]
    public void NaoDeveAutenticar_QuandoSenhaEstaIncorreta()
    {
        var usuario = new Usuario(Guid.NewGuid(), "user@teste.com", "hash-armazenado");

        var resultado = usuario.Autenticar("senha-errada", new HasherFakeQueSempreNega());

        Assert.False(resultado);
    }
}
