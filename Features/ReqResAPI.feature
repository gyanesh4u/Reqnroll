Feature: ReqRes API Users Endpoint
  As a QA engineer
  I want to test the ReqRes API users endpoint
  So that I can verify the API functionality and data integrity

  Background:
    Given the ReqRes API is available at https://reqres.in
    And the API key header is set to "x-api-key: reqres-free-v1"

  Scenario: Get Users from Page 2
    When I request the users list from page 2
    Then the response status should be 200
    And the response should contain a page field with value 2
    And the response should contain a data array with users
    And the data array should have 6 items

  Scenario: Verify User Data Structure
    When I request the users list from page 2
    Then the response status should be 200
    And each user in the data array should have the following fields:
      | Field      |
      | id         |
      | email      |
      | first_name |
      | last_name  |
      | avatar     |
    And the data array should contain at least one user

  Scenario: Verify Pagination Structure
    When I request the users list from page 2
    Then the response status should be 200
    And the response should have pagination fields:
      | Field       |
      | page        |
      | per_page    |
      | total       |
      | total_pages |
    And the data array should not be empty

  Scenario: Find User by ID
    When I request the users list from page 2
    Then the response status should be 200
    And I should find a user with id 8 in the data array
    And that user should have an email address
    And the email should contain "@reqres.in"
    And the user email should be "lindsay.ferguson@reqres.in"
