#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/AlertEvent/org/#{orgId}/action/Notify"
param = {
  Status: "firing",
  Receiver: "test-receiver",
  GroupKey: "test-group-key",
  Alerts: [
    {
      Status: "firing",
      Labels: {
        alertname: "Test Alert",
        severity: "warning"
      },
      Annotations: {
        summary: "This is a test alert summary",
        description: "This is a test alert description"
      }
    }
  ]
}

ENV['API_KEY'] = nil # Disable API Key for this test

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
