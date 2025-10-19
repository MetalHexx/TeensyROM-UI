describe('teensyrom-ui-e2e', () => {
  beforeEach(() => cy.visit('/'));

  it('should display finding devices text in busy dialog', () => {
    // Look for the busy dialog title containing "Finding Devices"
    cy.get('[mat-dialog-title]').should('contain', 'Finding Devices');
  });

  it('should wait for loading to complete and show device view', () => {
    // Wait for the busy dialog to disappear (loading completes)
    // Using a longer timeout since device discovery can take several seconds
    cy.get('[mat-dialog-title]', { timeout: 10000 }).should('not.exist');

    // Verify the device-view is now visible
    cy.get('.device-view', { timeout: 10000 }).should('be.visible');

    // Verify either devices list or "No devices found" message appears
    cy.get('.device-list', { timeout: 10000 }).should('exist');
  });
});
