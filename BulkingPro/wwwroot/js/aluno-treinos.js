/**
 * BULKINGPRO - ALUNO TREINOS MODERNOS
 * Gerenciamento de cards expansíveis, checklists e anotações
 */

(function() {
    'use strict';

    // ============================================
    // ESTADO GLOBAL
    // ============================================
    const concluidosPorDia = {};

    // ============================================
    // INICIALIZAÇÃO
    // ============================================
    document.addEventListener('DOMContentLoaded', function() {
        inicializarCards();
        carregarChecklists();
        carregarAnotacoes();
        atualizarResumos();
    });

    // ============================================
    // CARDS EXPANSÍVEIS
    // ============================================
    function inicializarCards() {
        const cards = document.querySelectorAll('.dia-card');
        
        cards.forEach((card, index) => {
            // Expandir card com clique
            const header = card.querySelector('.dia-header');
            if (header) {
                header.addEventListener('click', (e) => {
                    // Evitar expandir se clicou em checkbox ou botão de anotação
                    if (e.target.closest('.checkbox-moderno') || 
                        e.target.closest('.anotacao-preview') ||
                        e.target.closest('.btn-anotacao')) {
                        return;
                    }
                    toggleCard(card);
                });
            }

            // Inicializar estado do card
            const diaIndex = card.dataset.diaIndex || index;
            carregarProgressoCard(card, diaIndex);
        });
    }

    function toggleCard(card) {
        const wasExpanded = card.classList.contains('expanded');
        
        // Fechar outros cards se quiser (opcional - comportamento tipo accordion)
        // if (!wasExpanded) {
        //     document.querySelectorAll('.dia-card.expanded').forEach(c => {
        //         if (c !== card) c.classList.remove('expanded');
        //     });
        // }
        
        card.classList.toggle('expanded');
    }

    // ============================================
    // CHECKLIST (Conclusão de exercícios)
    // ============================================
    function carregarChecklists() {
        const checkboxes = document.querySelectorAll('.checkbox-moderno input[type="checkbox"]');
        
        checkboxes.forEach(checkbox => {
            const exercicioId = checkbox.dataset.exercicioId;
            const diaIndex = checkbox.dataset.diaIndex;
            const chave = `${diaIndex}_${exercicioId}`;
            
            // Carregar estado salvo
            const salvo = localStorage.getItem(`bulkingpro_exercicio_${chave}`);
            if (salvo === 'true') {
                checkbox.checked = true;
                marcarExercicioConcluido(checkbox.closest('.exercicio-card-moderno-aluno'), true);
            }
            
            // Adicionar evento
            checkbox.addEventListener('change', function() {
                const card = this.closest('.exercicio-card-moderno-aluno');
                const isChecked = this.checked;
                
                marcarExercicioConcluido(card, isChecked);
                localStorage.setItem(`bulkingpro_exercicio_${chave}`, isChecked);
                
                // Atualizar progresso do dia
                const diaCard = this.closest('.dia-card');
                const diaIdx = diaCard.dataset.diaIndex;
                atualizarProgressoDia(diaCard, diaIdx);
                
                // Atualizar resumo do dia
                atualizarResumoDia(diaCard);
            });
        });
    }
    
    function marcarExercicioConcluido(card, isConcluido) {
        if (!card) return;
        
        if (isConcluido) {
            card.classList.add('completed');
            // Adicionar efeito visual
            card.style.transform = 'scale(0.99)';
            setTimeout(() => {
                card.style.transform = '';
            }, 150);
        } else {
            card.classList.remove('completed');
        }
    }
    
    function carregarProgressoCard(card, diaIndex) {
        const checkboxes = card.querySelectorAll('.checkbox-moderno input[type="checkbox"]');
        let total = checkboxes.length;
        let concluidos = 0;
        
        checkboxes.forEach(cb => {
            const exercicioId = cb.dataset.exercicioId;
            const chave = `${diaIndex}_${exercicioId}`;
            if (localStorage.getItem(`bulkingpro_exercicio_${chave}`) === 'true') {
                concluidos++;
            }
        });
        
        const percentual = total > 0 ? (concluidos / total) * 100 : 0;
        const fill = card.querySelector('.progress-bar-fill');
        const text = card.querySelector('.progress-text');
        
        if (fill) fill.style.width = `${percentual}%`;
        if (text) text.textContent = `${concluidos}/${total} exercícios concluídos`;
        
        // Salvar no estado global
        concluidosPorDia[diaIndex] = { concluidos, total };
    }
    
    function atualizarProgressoDia(card, diaIndex) {
        const checkboxes = card.querySelectorAll('.checkbox-moderno input[type="checkbox"]');
        let total = checkboxes.length;
        let concluidos = 0;
        
        checkboxes.forEach(cb => {
            if (cb.checked) concluidos++;
        });
        
        const percentual = total > 0 ? (concluidos / total) * 100 : 0;
        const fill = card.querySelector('.progress-bar-fill');
        const text = card.querySelector('.progress-text');
        
        if (fill) fill.style.width = `${percentual}%`;
        if (text) text.textContent = `${concluidos}/${total} exercícios concluídos`;
        
        concluidosPorDia[diaIndex] = { concluidos, total };
    }

    // ============================================
    // RESUMO DO DIA
    // ============================================
    function atualizarResumos() {
        document.querySelectorAll('.dia-card').forEach((card, idx) => {
            atualizarResumoDia(card);
        });
    }
    
    function atualizarResumoDia(card) {
        const resumoDiv = card.querySelector('.resumo-dia');
        if (!resumoDiv) return;
        
        const checkboxes = card.querySelectorAll('.checkbox-moderno input[type="checkbox"]');
        let total = checkboxes.length;
        let concluidos = 0;
        
        checkboxes.forEach(cb => {
            if (cb.checked) concluidos++;
        });
        
        const percentual = total > 0 ? Math.round((concluidos / total) * 100) : 0;
        const fill = resumoDiv.querySelector('.progress-bar-fill');
        const mensagemSpan = resumoDiv.querySelector('.resumo-mensagem');
        
        if (fill) fill.style.width = `${percentual}%`;
        
        if (mensagemSpan) {
            const emoji = document.createElement('i');
            let texto = '';
            let classe = '';
            
            if (percentual === 100) {
                emoji.className = 'ti ti-circle-check';
                texto = 'Treino completo! Parabéns! 🎉';
                classe = 'concluido';
            } else if (percentual >= 70) {
                emoji.className = 'ti ti-trending-up';
                texto = `${percentual}% concluído! Continue assim! 💪`;
                classe = 'parcial';
            } else if (percentual >= 50) {
                emoji.className = 'ti ti-hourglass';
                texto = `${percentual}% concluído. Metade do caminho! 🏃`;
                classe = 'parcial';
            } else if (percentual > 0) {
                emoji.className = 'ti ti-alarm';
                texto = `${percentual}% concluído. Vamos finalizar! 🔥`;
                classe = 'parcial';
            } else {
                emoji.className = 'ti ti-barbell';
                texto = 'Nenhum exercício concluído ainda. Comece agora! 💪';
                classe = 'parcial';
            }
            
            mensagemSpan.innerHTML = '';
            mensagemSpan.appendChild(emoji);
            mensagemSpan.appendChild(document.createTextNode(` ${texto}`));
            mensagemSpan.className = `resumo-mensagem ${classe}`;
        }
        
        // Atualizar badge do card
        const badgeConclusao = card.querySelector('.progress-text');
        if (badgeConclusao && total > 0) {
            const badgePai = badgeConclusao.closest('.dia-meta');
            if (badgePai && percentual === 100) {
                if (!badgePai.querySelector('.concluido-badge')) {
                    const span = document.createElement('span');
                    span.className = 'concluido-badge';
                    span.innerHTML = '<i class="ti ti-circle-check"></i> Concluído!';
                    span.style.color = '#2ea043';
                    badgePai.appendChild(span);
                }
            } else {
                const badgeExistente = badgePai?.querySelector('.concluido-badge');
                if (badgeExistente) badgeExistente.remove();
            }
        }
    }

    // ============================================
    // ANOTAÇÕES DOS EXERCÍCIOS
    // ============================================
    function carregarAnotacoes() {
        const anotacoesPreviews = document.querySelectorAll('.anotacao-preview');
        
        anotacoesPreviews.forEach(preview => {
            const exercicioId = preview.dataset.exercicioId;
            const diaIndex = preview.dataset.diaIndex;
            const chave = `bulkingpro_anotacao_${diaIndex}_${exercicioId}`;
            
            // Carregar anotação salva
            const anotacaoSalva = localStorage.getItem(chave);
            if (anotacaoSalva && anotacaoSalva.trim() !== '') {
                const textoSpan = preview.querySelector('span');
                if (textoSpan) {
                    textoSpan.textContent = anotacaoSalva.length > 50 
                        ? anotacaoSalva.substring(0, 47) + '...' 
                        : anotacaoSalva;
                    textoSpan.style.color = 'var(--gold)';
                }
                preview.classList.remove('vazio');
                preview.style.opacity = '1';
            }
            
            // Adicionar evento de clique
            preview.addEventListener('click', function(e) {
                e.stopPropagation();
                abrirEditorAnotacao(preview, chave);
            });
        });
    }
    
    function abrirEditorAnotacao(preview, chave) {
        const editor = preview.nextElementSibling;
        const textarea = editor?.querySelector('.anotacao-textarea');
        const textoAtual = preview.querySelector('span')?.textContent || '';
        
        if (editor && textarea) {
            textarea.value = textoAtual !== 'Clique para adicionar anotação...' ? textoAtual : '';
            preview.style.display = 'none';
            editor.classList.add('active');
            textarea.focus();
            
            // Configurar botões
            const salvarBtn = editor.querySelector('.btn-anotacao.salvar');
            const cancelarBtn = editor.querySelector('.btn-anotacao.cancelar');
            
            if (salvarBtn) {
                const newSalvarBtn = salvarBtn.cloneNode(true);
                salvarBtn.parentNode.replaceChild(newSalvarBtn, salvarBtn);
                newSalvarBtn.addEventListener('click', () => salvarAnotacao(preview, editor, textarea, chave));
            }
            
            if (cancelarBtn) {
                const newCancelarBtn = cancelarBtn.cloneNode(true);
                cancelarBtn.parentNode.replaceChild(newCancelarBtn, cancelarBtn);
                newCancelarBtn.addEventListener('click', () => cancelarAnotacao(preview, editor));
            }
        }
    }
    
    function salvarAnotacao(preview, editor, textarea, chave) {
        const novoTexto = textarea.value.trim();
        const textoSpan = preview.querySelector('span');
        
        if (novoTexto !== '') {
            localStorage.setItem(chave, novoTexto);
            if (textoSpan) {
                textoSpan.textContent = novoTexto.length > 50 ? novoTexto.substring(0, 47) + '...' : novoTexto;
                textoSpan.style.color = 'var(--gold)';
            }
            preview.classList.remove('vazio');
            preview.style.opacity = '1';
        } else {
            localStorage.removeItem(chave);
            if (textoSpan) {
                textoSpan.textContent = 'Clique para adicionar anotação...';
                textoSpan.style.color = 'var(--muted)';
            }
            preview.classList.add('vazio');
            preview.style.opacity = '0.7';
        }
        
        preview.style.display = 'flex';
        editor.classList.remove('active');
        
        // Mostrar toast de confirmação
        mostrarToast('Anotação salva!', false);
    }
    
    function cancelarAnotacao(preview, editor) {
        preview.style.display = 'flex';
        editor.classList.remove('active');
    }
    
    function mostrarToast(mensagem, isError) {
        const toastExistente = document.querySelector('.toast-anotacao');
        if (toastExistente) toastExistente.remove();
        
        const toast = document.createElement('div');
        toast.className = 'toast-anotacao';
        toast.innerHTML = `<i class="ti ti-${isError ? 'alert-circle' : 'circle-check'}"></i> ${mensagem}`;
        toast.style.cssText = `
            position: fixed;
            bottom: 30px;
            right: 30px;
            background: ${isError ? '#ee1920' : '#2ea043'};
            color: #fff;
            padding: 12px 20px;
            border-radius: 10px;
            font-size: 13px;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 10px;
            z-index: 1000;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
            animation: fadeIn 0.3s ease;
        `;
        document.body.appendChild(toast);
        
        setTimeout(() => {
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 300);
        }, 2500);
    }
    
    // Adicionar keyframes dinamicamente
    const style = document.createElement('style');
    style.textContent = `
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(20px); }
            to { opacity: 1; transform: translateY(0); }
        }
        .toast-anotacao {
            animation: fadeIn 0.3s ease;
        }
    `;
    document.head.appendChild(style);
})();