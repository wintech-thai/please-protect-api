#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'a5fa3604-d8e3-4ebc-a978-09bd4bcf1782'

apiUrl = "api/Document/org/#{orgId}/action/GetDocumentById/#{id}"
param = nil

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
