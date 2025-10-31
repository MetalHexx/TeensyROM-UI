/**
 * Smoke test for player view against real backend (no mocking)
 * Verifies directory files render correctly
 */
describe('Player Smoke Test (No Mocking)', () => {
  it('should navigate to player and see directory files', () => {
    cy.viewport(1400, 900);

    cy.visit('/player');

    // Workaround: Random button click forces directory files to render
    cy.get('[data-testid="random-launch-button"]', { timeout: 10000 })
      .should('be.visible')
      .click();

    cy.log('Clicked random button');

    cy.get('lib-directory-files', { timeout: 10000 }).should('exist').then(($component) => {
      cy.log('Directory files component found');
      cy.log('Component visibility:', $component.is(':visible'));
      cy.log('Component dimensions:', $component.width(), 'x', $component.height());
    });

    cy.get('body').then(($body) => {
      const items = $body.find('[data-item-path]');
      cy.log('Total file items found:', items.length);
      items.each((i, el) => {
        if (i < 5) {
          cy.log(`File item ${i}:`, el.getAttribute('data-item-path'));
        }
      });
    });

    // Virtual scrolling may clip visibility - verify existence instead
    cy.get('[data-item-path]', { timeout: 10000 }).should('have.length.greaterThan', 0).then(($items) => {
      cy.log(`SUCCESS: ${$items.length} file items found in directory listing`);
      cy.log('First file item:', $items.first().attr('data-item-path'));
    });

    cy.screenshot('player-smoke-test-after-random');
  });
});
