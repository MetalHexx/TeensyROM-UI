/**
 * Simple smoke test for player view with NO mocking
 * Tests against real backend to see if directory files render
 */
describe('Player Smoke Test (No Mocking)', () => {
  it('should navigate to player and see directory files', () => {
    // Set viewport to ensure proper layout (default is 1000x660 which triggers responsive mode)
    cy.viewport(1400, 900);

    // Just visit the player page
    cy.visit('/player');


    // Click the random button to trigger directory files to load
    // (Workaround: random button click forces directory files to render)
    cy.get('[data-testid="random-launch-button"]', { timeout: 10000 })
      .should('be.visible')
      .click();

    cy.log('Clicked random button');

    // Look for the directory-files component
    cy.get('lib-directory-files', { timeout: 10000 }).should('exist').then(($component) => {
      cy.log('Directory files component found');
      cy.log('Component visibility:', $component.is(':visible'));
      cy.log('Component dimensions:', $component.width(), 'x', $component.height());
    });

    // Look for any file items
    cy.get('body').then(($body) => {
      const items = $body.find('[data-item-path]');
      cy.log('Total file items found:', items.length);
      items.each((i, el) => {
        if (i < 5) { // Only log first 5
          cy.log(`File item ${i}:`, el.getAttribute('data-item-path'));
        }
      });
    });

    // Verify file items exist (not strict visibility due to virtual scrolling clipping)
    cy.get('[data-item-path]', { timeout: 10000 }).should('have.length.greaterThan', 0).then(($items) => {
      cy.log(`SUCCESS: ${$items.length} file items found in directory listing`);
      cy.log('First file item:', $items.first().attr('data-item-path'));
    });

    // Take a screenshot for manual inspection
    cy.screenshot('player-smoke-test-after-random');
  });
});
