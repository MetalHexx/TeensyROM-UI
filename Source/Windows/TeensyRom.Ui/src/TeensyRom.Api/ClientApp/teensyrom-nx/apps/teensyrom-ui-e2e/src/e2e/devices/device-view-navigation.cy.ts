describe('Device View - Navigation', () => {
  beforeEach(() => {
    cy.visit('/devices');
  });

  it('should navigate to device view', () => {
    // Verify URL contains /devices
    cy.url().should('include', '/devices');

    // Verify device view container is present in DOM
    cy.get('.device-view').should('exist');
  });
});
