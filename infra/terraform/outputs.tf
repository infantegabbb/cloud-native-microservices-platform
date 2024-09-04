output "namespace" {
  value = kubernetes_namespace.app.metadata[0].name
}

output "order_service_name" {
  value = kubernetes_service.order.metadata[0].name
}
