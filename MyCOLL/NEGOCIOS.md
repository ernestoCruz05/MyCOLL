## Regras de Negócio
##### RN01 - Gestão de Catálogo e Produtos
1.  Propriedade: Um produto está sempre associado a um Fornecedor (ou à própria loja). Um fornecedor só pode editar ou eliminar os seus produtos.
2.  Aprovação: Produtos inseridos por fornecedores entram no sistema com estado Inativo (Pendente). Apenas um Gestor ou Admin pode alterar o estado para Ativo para que este fique visível na loja pública.
	
3.  Definição de Preço: O preço final de venda é definido pela loja. O sistema deve permitir armazenar o preço base (custo) e calcular o preço de venda ao público.
  
Imagens: Todo o produto deve ter pelo menos uma imagem associada para ser listado

##### RN02 - Integridade de Dados
1.  Eliminação Segura: Não é permitido eliminar fisicamente (Delete) categorias, produtos ou modos de entrega que já tenham sido utilizados em encomendas passadas.
	  -   Solução: O sistema deve impedir a ação e sugerir a "Inativação" (Soft Delete) através do campo Ativo = false.
2. Histórico de Preços: O valor de um produto numa encomenda deve ser o valor no momento da compra, e não o valor atual do catálogo. A tabela de DetalheEncomenda deve guardar uma cópia do preço unitário.

##### RN03 - Gestão de Encomendas e Stock
1.  Estados da Encomenda: Uma encomenda segue o fluxo estrito: Pendente -> Paga -> Expedida -> Entregue (ou Cancelada)

2.  Movimentação de Stock:
	- O stock do produto é decrementado apenas quando a encomenda passa para o estado Expedida.
	- Se uma encomenda expedida for Cancelada, o stock deve ser reposto automaticamente.
	- O sistema não deve permitir a expedição se o stock for insuficiente (Stock < Quantidade Encomendada).
	
3. Custos de Envio: Ao total dos produtos deve ser somado o custo do Modo de Entrega selecionado.

##### RN04 - Gestão de Utilizadores
1.  Auto-preservação: Um utilizador não pode alterar o seu próprio Role (ex: um Admin não pode despromover-se a si próprio acidentalmente).

3.  Bloqueio: O Admin deve poder bloquear o acesso (Login) a qualquer utilizador (exceto a si próprio) sem apagar os dados históricos desse utilizador.


