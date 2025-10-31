describe('teensyrom-ui-e2e', () => {
  beforeEach(() => cy.visit('/'));

  it('should display finding devices text in busy dialog', () => {
    cy.get('[mat-dialog-title]').should('contain', 'Finding Devices');
  });

  it('should wait for loading to complete and show device view', () => {
    // Device discovery can take several seconds, hence the extended timeout
    cy.get('[mat-dialog-title]', { timeout: 10000 }).should('not.exist');
    cy.get('.device-view', { timeout: 10000 }).should('be.visible');
    cy.get('.device-list', { timeout: 10000 }).should('exist');
  });
});
