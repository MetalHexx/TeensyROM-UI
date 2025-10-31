describe('Device View - Navigation', () => {
  beforeEach(() => {
    cy.visit('/devices');
  });

  it('should navigate to device view', () => {
    cy.url().should('include', '/devices');
    cy.get('.device-view').should('exist');
  });
});
