const api = "/api/pessoas";
const el = (id) => document.getElementById(id);

const campoNome = el("nome");
const campoSobrenome = el("sobrenome");
const campoDocumento = el("documento");
const resumo = el("resumo");
const botaoSalvar = el("btn-salvar");
const botaoConcorrencia = el("btn-concorrencia");
const botaoLimpar = el("btn-limpar");
const opcoesEstrategia = Array.from(document.querySelectorAll(".estrategia"));

const nomesEstrategia = {
    "sem-protecao": "Sem proteção",
    "concurrent-dictionary": "ConcurrentDictionary",
    "semaphore": "SemaphoreSlim",
    "lock": "lock / Monitor",
    "memory-cache": "MemoryCache"
};

let executando = false;

function obterEstrategia() {
    const selecionada = opcoesEstrategia.find((opcao) => opcao.checked);
    return selecionada ? selecionada.value : "sem-protecao";
}

opcoesEstrategia.forEach((opcao) => {
    opcao.addEventListener("change", () => {
        if (!opcao.checked) {
            opcao.checked = true;
            return;
        }

        opcoesEstrategia.forEach((outra) => {
            if (outra !== opcao) {
                outra.checked = false;
            }
        });
    });
});

function montarPayload() {
    const payload = {
        nome: campoNome.value.trim(),
        sobrenome: campoSobrenome.value.trim(),
        documento: campoDocumento.value.trim()
    };

    if (!payload.nome) {
        campoNome.focus();
        return null;
    }

    if (!payload.sobrenome) {
        campoSobrenome.focus();
        return null;
    }

    if (!payload.documento) {
        campoDocumento.focus();
        return null;
    }

    return payload;
}

async function salvar(payload) {
    const estrategia = obterEstrategia();
    const resposta = await fetch(`${api}?estrategia=${estrategia}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    });

    return resposta.status;
}

function contarDuplicados(pessoas) {
    const contagem = new Map();

    for (const pessoa of pessoas) {
        const chave = pessoa.documento.toLowerCase();
        contagem.set(chave, (contagem.get(chave) || 0) + 1);
    }

    return new Set(
        [...contagem.entries()]
            .filter(([, total]) => total > 1)
            .map(([chave]) => chave)
    );
}

function criarCelula(texto) {
    const td = document.createElement("td");
    td.textContent = texto;
    return td;
}

function renderTabela(pessoas) {
    const corpo = el("corpo-tabela");
    const duplicados = contarDuplicados(pessoas);

    corpo.innerHTML = "";

    for (const pessoa of pessoas) {
        const linha = document.createElement("tr");
        const documentoDuplicado = duplicados.has(pessoa.documento.toLowerCase());

        if (documentoDuplicado) {
            linha.classList.add("duplicado");
        }

        linha.appendChild(criarCelula(pessoa.nome));
        linha.appendChild(criarCelula(pessoa.sobrenome));

        const celulaDocumento = criarCelula(pessoa.documento);
        if (documentoDuplicado) {
            const tag = document.createElement("span");
            tag.className = "tag";
            tag.textContent = "duplicado";
            celulaDocumento.appendChild(tag);
        }
        linha.appendChild(celulaDocumento);

        linha.appendChild(criarCelula(
            new Date(pessoa.criadoEm).toLocaleTimeString("pt-BR")
        ));

        corpo.appendChild(linha);
    }

    el("vazio").classList.toggle("oculto", pessoas.length > 0);
    el("contador").textContent = `${pessoas.length} ${pessoas.length === 1 ? "registro" : "registros"}`;

    return duplicados.size;
}

async function carregarPessoas() {
    const resposta = await fetch(api);
    const pessoas = await resposta.json();
    const documentosDuplicados = renderTabela(pessoas);
    return documentosDuplicados;
}

function mostrarResumo({ total, sucesso, conflito, duracao, documentosDuplicados, estrategia }) {
    resumo.classList.remove("oculto");
    resumo.textContent = [
        `${total} requisições`,
        `${sucesso} cadastro(s)`,
        `${conflito} conflito(s)`,
        `${duracao} ms`,
        nomesEstrategia[estrategia] || estrategia
    ].join(" · ");

    const situacao = document.createElement("span");
    if (documentosDuplicados > 0) {
        situacao.className = "alerta";
        situacao.textContent = `${documentosDuplicados} documento(s) duplicado(s)`;
    } else {
        situacao.className = "ok";
        situacao.textContent = "sem duplicatas";
    }

    resumo.append(" · ", situacao);
}

function definirBotoesHabilitados(habilitado) {
    botaoSalvar.disabled = !habilitado;
    botaoConcorrencia.disabled = !habilitado;
    botaoLimpar.disabled = !habilitado;
}

async function rodarLote(quantidade) {
    if (executando) {
        return;
    }

    const payload = montarPayload();
    if (!payload) {
        return;
    }

    const estrategia = obterEstrategia();
    executando = true;
    definirBotoesHabilitados(false);

    try {
        const inicio = performance.now();
        const status = await Promise.all(
            Array.from({ length: quantidade }, () => salvar(payload))
        );
        const duracao = Math.round(performance.now() - inicio);

        const sucesso = status.filter((codigo) => codigo === 201).length;
        const conflito = status.filter((codigo) => codigo === 409).length;

        const documentosDuplicados = await carregarPessoas();

        mostrarResumo({
            total: status.length,
            sucesso,
            conflito,
            duracao,
            documentosDuplicados,
            estrategia
        });
    } finally {
        executando = false;
        definirBotoesHabilitados(true);
    }
}

el("form-cadastro").addEventListener("submit", (evento) => {
    evento.preventDefault();
    rodarLote(1);
});

botaoConcorrencia.addEventListener("click", () => {
    rodarLote(10);
});

botaoLimpar.addEventListener("click", async () => {
    if (executando) {
        return;
    }

    executando = true;
    definirBotoesHabilitados(false);

    try {
        await fetch(`${api}/limpar`, { method: "POST" });
        await carregarPessoas();
        resumo.classList.add("oculto");
        resumo.textContent = "";
    } finally {
        executando = false;
        definirBotoesHabilitados(true);
    }
});

carregarPessoas();
